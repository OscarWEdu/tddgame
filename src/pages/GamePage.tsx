import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { toast } from "sonner";

import { useGetApiTerritories } from "@/api/generated/territories/territories";
import RiskMap, { nameToSvgId } from "@/components/RiskMap";

GamePage.route = {
  path: "/game/:sessionId",
};

export default function GamePage() {
  const { sessionId } = useParams<{ sessionId: string }>();
  const { data, isLoading, isError } = useGetApiTerritories();
  const [selectedSvgId, setSelectedSvgId] = useState<string | null>(null);

  const territories = data?.data ?? [];
  const selectedTerritory =
    selectedSvgId !== null
      ? territories.find((t) => nameToSvgId(t.name) === selectedSvgId) ?? null
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
            />
          </div>
          {/* Game Decisions */}
          <div className="flex w-80 flex-col gap-3 rounded border bg-white p-4">
            <p className="font-semibold">Game Status</p>
            {isLoading && (
              <p className="text-sm text-slate-500">Loading territories…</p>
            )}
            {isError && (
              <p className="text-sm text-red-600">Could not get territories</p>
            )}
            {!isLoading && !isError && (
              <p className="text-sm text-slate-500">
                {territories.length} territorier loaded
              </p>
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
                    <dt className="text-slate-500">North</dt>
                    <dd>{selectedTerritory.northAdjacentId}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">South</dt>
                    <dd>{selectedTerritory.southAdjacentId}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">East</dt>
                    <dd>{selectedTerritory.eastAdjacentId}</dd>
                  </div>
                  <div className="flex justify-between">
                    <dt className="text-slate-500">West</dt>
                    <dd>{selectedTerritory.westAdjacentId}</dd>
                  </div>
                </dl>
                <button
                  type="button"
                  onClick={() => setSelectedSvgId(null)}
                  className="mt-2 w-full rounded border border-slate-300 px-2 py-1 text-xs text-slate-600 hover:bg-slate-100"
                >
                  Avmarkera
                </button>
              </div>
            ) : (
              <p className="text-sm italic text-slate-400">
                Click on a territory to see details here.
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
