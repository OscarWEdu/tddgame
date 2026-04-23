import { CustomDialog } from "@/components/custom/CustomDialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useEffect, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { toast } from "sonner";
import {
  useDeleteApiGameSessionId,
  useGetApiGameSessionId,
  usePatchApiGameSessionIdStatus,
} from "../api/generated/game-sessions/game-sessions";
import { GameSessionStatus } from "../api/generated/models/gameSessionStatus";
import type { PlayerDto } from "../api/generated/models/playerDto";
import {
  useDeleteApiPlayersId,
  useGetApiPlayers,
} from "../api/generated/players/players";

export default function LobbyPage() {
  const navigate = useNavigate();
  const { sessionId } = useParams<{ sessionId: string }>();
  const storedId = sessionId
    ? sessionStorage.getItem(`player_${sessionId}`)
    : null;
  const playerId: number | undefined = storedId ? Number(storedId) : undefined;

  const {
    data: sessionData,
    isLoading: sessionLoading,
    refetch: refetchSession,
  } = useGetApiGameSessionId(sessionId ?? "", {
    query: { refetchInterval: 3000, enabled: !!sessionId },
  });

  const {
    data: playersData,
    isLoading: playersLoading,
    refetch: refetchPlayers,
  } = useGetApiPlayers(
    { gameSessionId: sessionId ?? "" },
    { query: { refetchInterval: 3000, enabled: !!sessionId } },
  );

  const statusMutation = usePatchApiGameSessionIdStatus();
  const kickMutation = useDeleteApiPlayersId();
  const deleteLobbyMutation = useDeleteApiGameSessionId();

  const session = sessionData?.status === 200 ? sessionData.data : null;
  const players: PlayerDto[] =
    playersData?.status === 200 ? playersData.data : [];
  const currentPlayer = players.find((p) => p.id === playerId);
  const isHost = currentPlayer?.isHost ?? false;

  const startedRef = useRef(false);
  const [playerToKick, setPlayerToKick] = useState<PlayerDto | null>(null);
  const [leaveOpen, setLeaveOpen] = useState(false);
  const [closeLobbyOpen, setCloseLobbyOpen] = useState(false);

  useEffect(() => {
    if (session?.status === GameSessionStatus.started && !startedRef.current) {
      startedRef.current = true;
      navigate(`/game/${sessionId}`);
    }
  }, [session?.status, sessionId, navigate]);

  useEffect(() => {
    if (
      !playersLoading &&
      playerId !== undefined &&
      players.length > 0 &&
      !currentPlayer
    ) {
      sessionStorage.removeItem(`player_${sessionId}`);
      navigate("/");
    }
  }, [playersLoading, playerId, players, currentPlayer, sessionId, navigate]);

  const handleStart = async () => {
    if (!sessionId) return;

    const response = await statusMutation.mutateAsync({
      id: sessionId,
      data: { status: GameSessionStatus.started },
    });

    if (response.status !== 200) {
      toast.error("Failed to start the game.");
      await refetchSession();
      return;
    }

    navigate(`/game/${sessionId}`);
  };

  const handleKickPlayer = async () => {
    if (!playerToKick) return;

    const response = await kickMutation.mutateAsync({ id: playerToKick.id });
    if (response.status !== 204) {
      toast.error("Failed to remove the player.");
      await refetchPlayers();
      return;
    }

    toast.success(`${playerToKick.name} was removed from the lobby.`);
    setPlayerToKick(null);
    await refetchPlayers();
  };

  const handleLeaveLobby = async () => {
    if (playerId === undefined || !sessionId) return;

    const response = await kickMutation.mutateAsync({ id: playerId });
    if (response.status !== 204) {
      toast.error("Failed to leave the lobby.");
      return;
    }

    sessionStorage.removeItem(`player_${sessionId}`);
    navigate("/");
  };

  const handleCloseLobby = async () => {
    if (!sessionId) return;

    const response = await deleteLobbyMutation.mutateAsync({ id: sessionId });
    if (response.status !== 204) {
      toast.error("Failed to disband the lobby.");
      await refetchSession();
      return;
    }

    sessionStorage.removeItem(`player_${sessionId}`);
    navigate("/");
  };

  const isLoading = sessionLoading || playersLoading;
  const isLeaving = kickMutation.isPending && playerToKick === null;

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
                    size="xs"
                    disabled={kickMutation.isPending}
                    onClick={() => setPlayerToKick(player)}
                  >
                    Kick
                  </Button>
                )}
              </div>
            </div>
          ))}
      </div>

      {isHost ? (
        <div className="flex justify-end gap-3">
          <Button
            variant="outline"
            disabled={deleteLobbyMutation.isPending || statusMutation.isPending}
            onClick={() => setCloseLobbyOpen(true)}
          >
            Disband Lobby
          </Button>
          <Button
            disabled={
              players.length < 2 ||
              statusMutation.isPending ||
              deleteLobbyMutation.isPending
            }
            onClick={handleStart}
          >
            {statusMutation.isPending ? "Starting..." : "Start Game"}
          </Button>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-3">
          <p className="text-center text-sm text-muted-foreground">
            Waiting for the host to start the game...
          </p>
          <Button
            variant="outline"
            disabled={isLeaving}
            onClick={() => setLeaveOpen(true)}
          >
            Leave Lobby
          </Button>
        </div>
      )}

      <CustomDialog
        open={playerToKick !== null}
        onOpenChange={(open) => {
          if (!open) {
            setPlayerToKick(null);
          }
        }}
        title="Remove player"
        description={`Remove ${playerToKick?.name ?? "this player"} from the lobby?`}
        onConfirm={handleKickPlayer}
        onCancel={() => setPlayerToKick(null)}
        confirmLabel="Remove player"
        isSubmitting={kickMutation.isPending}
        submittingLabel="Removing..."
      >
        <p className="text-sm text-muted-foreground">
          They will be sent back to the home screen and will need to join again.
        </p>
      </CustomDialog>

      <CustomDialog
        open={leaveOpen}
        onOpenChange={setLeaveOpen}
        title="Leave lobby"
        description="Leave this lobby and return to the home screen?"
        onConfirm={handleLeaveLobby}
        onCancel={() => setLeaveOpen(false)}
        confirmLabel="Leave Lobby"
        isSubmitting={isLeaving}
        submittingLabel="Leaving..."
      >
        <p className="text-sm text-muted-foreground">
          You will lose your spot in the lobby.
        </p>
      </CustomDialog>

      <CustomDialog
        open={closeLobbyOpen}
        onOpenChange={setCloseLobbyOpen}
        title="Disband lobby"
        description="Disband this lobby for everyone and return to the home screen?"
        onConfirm={handleCloseLobby}
        onCancel={() => setCloseLobbyOpen(false)}
        confirmLabel="Disband Lobby"
        isSubmitting={deleteLobbyMutation.isPending}
        submittingLabel="Disbanding..."
      >
        <p className="text-sm text-muted-foreground">
          This will remove the whole lobby, including all players waiting in it.
        </p>
      </CustomDialog>
    </div>
  );
}
