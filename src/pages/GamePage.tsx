import { useEffect, useMemo, useRef, useState } from "react";
import { useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import { useGetApiPlayers } from "@/api/generated/players/players";
import { useGetApiTerritories } from "@/api/generated/territories/territories";
import type { PlayerDto } from "@/api/generated/models/playerDto";
import type { PlayerTerritoryDto } from "@/api/generated/models/playerTerritoryDto";
import RiskMap, { nameToSvgId } from "@/components/RiskMap";

GamePage.route = {
  path: "/game/:sessionId",
};

const playerColours: Record<string, string> = {
  Black: "#1f2937",
  Blue: "#2563eb",
  Green: "#16a34a",
  Pink: "#db2777",
  Red: "#b91c1c",
  Yellow: "#eab308",
};

const backupFill = "#475569";

export default function GamePage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const queryClient = useQueryClient();
  const [selectedSvgId, setSelectedSvgId] = useState<string | null>(null);

  const { data: territoriesData, isLoading, isError } = useGetApiTerritories();
  const { data: playersData } = useGetApiPlayers(
    { gameSessionId: sessionId ?? "" },
    { query: { enabled: !!sessionId } },
  );
  const territories = territoriesData?.data ?? [];
  const players: PlayerDto[] = playersData?.data ?? [];

  const ownershipQueryKey = ["playerTerritories", sessionId] as const;
  const { data: ownership = [] } = useQuery({
    queryKey: ownershipQueryKey,
    queryFn: async (): Promise<PlayerTerritoryDto[]> => {
      const results = await Promise.all(
        players.map((p) =>
          fetch(`/api/playerterritories/player/${p.id}`).then((r) => {
            if (!r.ok)
              throw new Error(`Failed to fetch territories for player ${p.id}`);
            return r.json() as Promise<PlayerTerritoryDto[]>;
          }),
        ),
      );
      return results.flat();
    },
    enabled: players.length > 0,
  });

  const assignMutation = useMutation({
    mutationFn: async (): Promise<PlayerTerritoryDto[]> => {
      const r = await fetch(
        `/api/playerterritories/assign-initial/${sessionId}`,
        { method: "POST" },
      );
      if (!r.ok) throw new Error("Failed to assign initial territories");
      return r.json();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ownershipQueryKey });
    },
  });

  const selectedTerritory =
    selectedSvgId !== null
      ? (territories.find((t) => nameToSvgId(t.name) === selectedSvgId) ?? null)
      : null;

  useEffect(() => {
    if (isLoading || territories.length === 0) {
      toast.warning("Fetching Territories");

      const timeout = setTimeout(() => {
        window.location.reload();
      }, 4500);

      return () => clearTimeout(timeout);
    }
  }, [isLoading, territories.length]);

  const assignedSessionsRef = useRef(new Set<string>());
  useEffect(() => {
    if (!sessionId) return;
    if (assignedSessionsRef.current.has(sessionId)) return;
    assignedSessionsRef.current.add(sessionId);
    assignMutation.mutate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sessionId]);

  const ownershipBySvgId = useMemo(() => {
    const playerById = new Map(players.map((p) => [p.id, p]));
    const territoryById = new Map(territories.map((t) => [t.id, t]));
    const map = new Map<string, { fill: string; player: PlayerDto }>();
    ownership.forEach((pt) => {
      const player = playerById.get(pt.playerId);
      const territory = territoryById.get(pt.territoryId);
      if (!player || !territory) return;
      map.set(nameToSvgId(territory.name), {
        fill: playerColours[player.colour] ?? backupFill,
        player,
      });
    });
    return map;
  }, [ownership, players, territories]);

  const selectedOwner = selectedSvgId
    ? (ownershipBySvgId.get(selectedSvgId)?.player ?? null)
    : null;

  return (
    <div className="min-h-screen w-full bg-slate-100 p-6">
      <div className="container mx-auto">
        <div className="text-center text-3xl font-bold mb-6">
          <h1>Typing Territory</h1>
        </div>
        <p className="mt-2 text-slate-600">Session id: {sessionId}</p>

        <div className="flex gap-6">
          {/* Game Map */}
          <div className="flex-1 rounded border bg-white p-2">
            <RiskMap
              territories={territories}
              selectedSvgId={selectedSvgId}
              onSelectChange={setSelectedSvgId}
              ownershipBySvgId={ownershipBySvgId}
            />
          </div>
          {/* Game Decisions */}
          <div className="flex w-80 flex-col gap-3 rounded border bg-white p-4">
            <p className="font-semibold">Game Status</p>
            {isLoading && (
              <p className="text-sm text-slate-500">Loading territories…</p>
            )}
            {isError && (
              <p className="text-sm text-red-600">Could not load territories</p>
            )}
            {!isLoading && !isError && (
              <p className="text-sm text-slate-500">
                {territories.length} territories · {ownership.length} owned
              </p>
            )}

            {players.length > 0 && (
              <>
                <hr className="border-slate-200" />
                <div className="space-y-1">
                  <p className="text-xs uppercase tracking-wide text-slate-500">
                    Spelare
                  </p>
                  {players.map((p) => {
                    const owned = ownership.filter(
                      (pt) => pt.playerId === p.id,
                    ).length;
                    const fill = playerColours[p.colour] ?? backupFill;
                    return (
                      <div
                        key={p.id}
                        className="flex items-center gap-2 text-sm"
                      >
                        <span
                          className="inline-block h-3 w-3 rounded-sm border border-slate-300"
                          style={{ backgroundColor: fill }}
                        />
                        <span className="flex-1 text-slate-700">{p.name}</span>
                        <span className="text-xs text-slate-500">
                          {owned} country
                        </span>
                      </div>
                    );
                  })}
                </div>
              </>
            )}

            <hr className="border-slate-200" />

            {selectedTerritory ? (
              <div className="space-y-1">
                <p className="text-xs uppercase tracking-wide text-slate-500">
                  Marked territory
                </p>
                <p className="text-lg font-semibold text-slate-900">
                  {selectedTerritory.name}
                </p>
                <dl className="space-y-1 text-sm text-slate-700">
                  <div className="flex justify-between">
                    <dt className="text-slate-500">ID</dt>
                    <dd>{selectedTerritory.id}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">Continent</dt>
                    <dd>{selectedTerritory.continentId}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">Owner</dt>
                    <dd>{selectedOwner?.name ?? "—"}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">Neighbors</dt>
                    <dd>{selectedTerritory.adjacentTerritoryIds.length} st</dd>
                  </div>
                </dl>
                <button
                  type="button"
                  onClick={() => setSelectedSvgId(null)}
                  className="mt-2 w-full rounded border border-slate-300 px-2 py-1 text-xs text-slate-600 hover:bg-slate-100"
                >
                  Unmark
                </button>
              </div>
            ) : (
              <p className="text-sm italic text-slate-400">
                Click on a territory to see information
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
