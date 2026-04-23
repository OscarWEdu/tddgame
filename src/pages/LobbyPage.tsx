import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useEffect, useRef } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  useGetApiGameSessionId,
  usePatchApiGameSessionIdStatus,
} from "../api/generated/game-sessions/game-sessions";
import { GameSessionStatus } from "../api/generated/models/gameSessionStatus";
import type { PlayerDto } from "../api/generated/models/playerDto";
import { useDeleteApiPlayersId, useGetApiPlayers } from "../api/generated/players/players";

export default function LobbyPage() {
  const navigate = useNavigate();
  const { sessionId } = useParams<{ sessionId: string }>();
  const storedId = sessionId ? localStorage.getItem(`player_${sessionId}`) : null;
  const playerId: number | undefined = storedId ? Number(storedId) : undefined;

  const { data: sessionData, isLoading: sessionLoading } = useGetApiGameSessionId(
    sessionId ?? "",
    { query: { refetchInterval: 3000, enabled: !!sessionId } }
  );

  const { data: playersData, isLoading: playersLoading } = useGetApiPlayers(
    { gameSessionId: sessionId ?? "" },
    { query: { refetchInterval: 3000, enabled: !!sessionId } }
  );

  const statusMutation = usePatchApiGameSessionIdStatus();
  const kickMutation = useDeleteApiPlayersId();

  const session = sessionData?.status === 200 ? sessionData.data : null;
  const players: PlayerDto[] = playersData?.status === 200 ? playersData.data : [];
  const currentPlayer = players.find((p) => p.id === playerId);
  const isHost = currentPlayer?.isHost ?? false;

  const startedRef = useRef(false);

  useEffect(() => {
    if (session?.status === GameSessionStatus.started && !startedRef.current) {
      startedRef.current = true;
      navigate(`/game/${sessionId}`);
    }
  }, [session?.status, sessionId, navigate]);

  useEffect(() => {
    if (!playersLoading && playerId !== undefined && players.length > 0 && !currentPlayer) {
      localStorage.removeItem(`player_${sessionId}`);
      navigate("/");
    }
  }, [playersLoading, playerId, players, currentPlayer, sessionId, navigate]);

  const handleStart = async () => {
    await statusMutation.mutateAsync({
      id: sessionId ?? "",
      data: { status: GameSessionStatus.started },
    });
    navigate(`/game/${sessionId}`);
  };

  const isLoading = sessionLoading || playersLoading;

  return (
    <div className="flex flex-col gap-6 py-8 bg-background/25 px-12 rounded-xl">
      <div className="flex items-center justify-between">
        <div>
          {sessionLoading ? (
            <Skeleton className="h-7 w-48" />
          ) : (
            <h1 className="text-2xl font-bold">{session?.name}</h1>
          )}
          {session && (
            <p className="text-sm text-muted-foreground mt-1">
              {players.length}/{session.maxPlayers} players
            </p>
          )}
        </div>
        <Badge variant="default">Lobby</Badge>
      </div>

      <div className="flex flex-col gap-2">
        <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
          Players
        </h2>
        {isLoading &&
          [1, 2, 3].map((k) => (
            <div
              key={k}
              className="flex items-center justify-between rounded-lg border border-border bg-card px-4 py-3"
            >
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-5 w-12 rounded-full" />
            </div>
          ))}
        {!isLoading && players.length === 0 && (
          <p className="text-sm text-muted-foreground py-4 text-center">
            No players yet.
          </p>
        )}
        {!isLoading &&
          players.map((player: PlayerDto) => (
            <div
              key={player.id}
              className="flex items-center justify-between rounded-lg border border-border bg-card px-4 py-3"
            >
              <div className="flex items-center gap-2">
                <span
                  className="size-3 rounded-full"
                  style={{ backgroundColor: player.colour.toLowerCase() }}
                />
                <span className="text-sm font-medium">
                  {player.name}
                  {player.id === playerId && (
                    <span className="text-muted-foreground"> (you)</span>
                  )}
                </span>
              </div>
              <div className="flex items-center gap-2">
                {player.isHost && <Badge variant="secondary">Host</Badge>}
                {isHost && !player.isHost && (
                  <Button
                    variant="destructive"
                    size="sm"
                    disabled={kickMutation.isPending}
                    onClick={() => kickMutation.mutate({ id: player.id })}
                  >
                    Kick
                  </Button>
                )}
              </div>
            </div>
          ))}
      </div>

      {isHost ? (
        <div className="flex justify-end">
          <Button
            disabled={players.length < 2 || statusMutation.isPending}
            onClick={handleStart}
          >
            {statusMutation.isPending ? "Starting..." : "Start Game"}
          </Button>
        </div>
      ) : (
        <p className="text-center text-sm text-muted-foreground">
          Waiting for the host to start the game…
        </p>
      )}
    </div>
  );
}
