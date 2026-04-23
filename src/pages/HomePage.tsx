import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { PlusIcon } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import {
  useGetApiGameSession,
  usePostApiGameSession,
} from "../api/generated/game-sessions/game-sessions";
import type { GameSessionDto } from "../api/generated/models/gameSessionDto";
import { GameSessionStatus } from "../api/generated/models/gameSessionStatus";
import { usePostApiPlayers } from "../api/generated/players/players";
import { CustomDialog } from "../components/custom/CustomDialog";

const defaultColours = ["Black", "Blue", "Green", "Pink", "Red", "Yellow"];
const minPlayers = 2;
const maxPlayers = 6;

const statusVariant: Record<
  string,
  "default" | "secondary" | "destructive" | "outline"
> = {
  [GameSessionStatus.lobby]: "default",
  [GameSessionStatus.started]: "secondary",
  [GameSessionStatus.completed]: "outline",
};

const statusLabel: Record<string, string> = {
  [GameSessionStatus.lobby]: "Lobby",
  [GameSessionStatus.started]: "In progress",
  [GameSessionStatus.completed]: "Completed",
};

const SKELETON_KEYS = ["a", "b", "c", "d", "e"];

function getJoinUnavailableMessage(session: GameSessionDto) {
  if (session.status !== GameSessionStatus.lobby) {
    return "This lobby is no longer accepting players.";
  }

  if (session.playerCount >= session.maxPlayers) {
    return "This lobby is already full.";
  }

  return null;
}

function SessionCardSkeleton() {
  return (
    <div className="flex items-center justify-between rounded-lg border border-border bg-card px-4 py-3">
      <Skeleton className="h-4 w-36" />
      <Skeleton className="h-5 w-16 rounded-full" />
    </div>
  );
}

export default function HomePage() {
  const { data, isLoading } = useGetApiGameSession();
  const gameSessionMutation = usePostApiGameSession();
  const playerMutation = usePostApiPlayers();
  const navigate = useNavigate();

  const [createOpen, setCreateOpen] = useState(false);
  const [joinSession, setJoinSession] = useState<GameSessionDto | null>(null);
  const [fullLobbySession, setFullLobbySession] =
    useState<GameSessionDto | null>(null);
  const [gameName, setGameName] = useState("");
  const [hostName, setHostName] = useState("");
  const [playerCount, setPlayerCount] = useState(minPlayers);
  const [joinName, setJoinName] = useState("");

  const sessions: GameSessionDto[] = data?.status === 200 ? data.data : [];
  const isCreating = gameSessionMutation.isPending || playerMutation.isPending;

  const handleCreateGame = async () => {
    const session = await gameSessionMutation.mutateAsync({
      data: { name: gameName, maxPlayers: playerCount },
    });
    if (session.status !== 201) {
      toast.error("Failed to create game session");
      return;
    }
    const sessionId = session.data.id;

    const player = await playerMutation.mutateAsync({
      data: {
        name: hostName.trim(),
        colour: defaultColours[0],
        turnOrder: 1,
        missionId: 1,
      },
      params: { gameSessionId: sessionId },
    });
    if (player.status !== 201) {
      toast.error("Failed to create player");
      return;
    }

    sessionStorage.setItem(`player_${sessionId}`, String(player.data.id));
    setGameName("");
    setHostName("");
    setPlayerCount(minPlayers);
    setCreateOpen(false);
    navigate(`/lobby/${sessionId}`);
  };

  const handleJoinGame = async () => {
    if (!joinSession) return;

    const joinUnavailableMessage = getJoinUnavailableMessage(joinSession);
    if (joinUnavailableMessage) {
      if (joinSession.playerCount >= joinSession.maxPlayers) {
        setFullLobbySession(joinSession);
      } else {
        toast.error(joinUnavailableMessage);
      }
      setJoinSession(null);
      return;
    }

    const targetSessionId = joinSession.id;
    try {
      const player = await playerMutation.mutateAsync({
        data: {
          name: joinName.trim(),
          colour:
            defaultColours[joinSession.playerCount % defaultColours.length],
          turnOrder: joinSession.playerCount + 1,
          missionId: 1,
        },
        params: { gameSessionId: targetSessionId },
      });
      if (player.status !== 201) {
        toast.error(
          typeof player.data === "string" ? player.data : "Failed to join game",
        );
        return;
      }
      sessionStorage.setItem(
        `player_${targetSessionId}`,
        String(player.data.id),
      );
      setJoinName("");
      setJoinSession(null);
      navigate(`/lobby/${targetSessionId}`);
    } catch {
      toast.error("Failed to join game");
    }
  };

  return (
    <div className="flex flex-col gap-6 py-8 bg-background/25 px-12 rounded-xl">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold text-accent-foreground">
          Available Games
        </h2>
        <Button
          size="sm"
          className="gap-2 bg-primary/80 rounded-md"
          onClick={() => setCreateOpen(true)}
        >
          <PlusIcon className="size-4" />
          Create Game
        </Button>
      </div>

      <ScrollArea>
        <div className="flex flex-col gap-2 pr-3 pt-1">
          {isLoading &&
            SKELETON_KEYS.map((key) => <SessionCardSkeleton key={key} />)}
          {!isLoading && sessions.length === 0 && (
            <p className="py-12 text-center text-sm text-muted-foreground">
              No games available. Create one!
            </p>
          )}
          {!isLoading &&
            sessions.map((session) => (
              <Card
                key={session.id}
                className="cursor-pointer p-1 rounded-md transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg hover:shadow-black/30 hover:border-border/60"
                onClick={() => {
                  const joinUnavailableMessage =
                    getJoinUnavailableMessage(session);
                  if (joinUnavailableMessage) {
                    if (session.playerCount >= session.maxPlayers) {
                      setFullLobbySession(session);
                    } else {
                      toast.error(joinUnavailableMessage);
                    }
                    return;
                  }

                  setJoinSession(session);
                }}
              >
                <CardContent className="flex items-center justify-between px-3 py-2">
                  <span className="text-sm font-medium text-foreground">
                    {session.name}
                  </span>
                  <div className="flex items-center gap-2">
                    <span className="text-xs text-muted-foreground">
                      {session.playerCount}/{session.maxPlayers}
                    </span>
                    {session.status && (
                      <Badge variant={statusVariant[session.status]}>
                        {statusLabel[session.status]}
                      </Badge>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
        </div>
      </ScrollArea>

      <CustomDialog
        open={createOpen}
        onOpenChange={setCreateOpen}
        title="Create a new game"
        description="Set up your game session and choose the number of players."
        onConfirm={handleCreateGame}
        confirmLabel="Create game"
        confirmDisabled={!gameName.trim() || !hostName.trim()}
        isSubmitting={isCreating}
        submittingLabel="Creating..."
      >
        <div className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="game-name">Game name</Label>
            <Input
              id="game-name"
              value={gameName}
              onChange={(e) => setGameName(e.target.value)}
              placeholder="My new game"
            />
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="host-name">Your name</Label>
            <Input
              id="host-name"
              value={hostName}
              onChange={(e) => setHostName(e.target.value)}
              placeholder="Enter your name"
            />
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="player-count">Max players</Label>
            <Select
              value={String(playerCount)}
              onValueChange={(v) => setPlayerCount(Number(v))}
            >
              <SelectTrigger id="player-count">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Array.from(
                  { length: maxPlayers - minPlayers + 1 },
                  (_, i) => i + minPlayers,
                ).map((n) => (
                  <SelectItem key={n} value={String(n)}>
                    {n} players
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>
      </CustomDialog>

      <CustomDialog
        open={joinSession !== null}
        onOpenChange={(open) => {
          if (!open) {
            setJoinSession(null);
            setJoinName("");
          }
        }}
        title="Join game"
        description={`Joining ${joinSession?.name} (${joinSession?.playerCount}/${joinSession?.maxPlayers} players)`}
        onConfirm={handleJoinGame}
        onCancel={() => {
          setJoinSession(null);
          setJoinName("");
        }}
        confirmLabel="Join"
        confirmDisabled={!joinName.trim()}
        isSubmitting={playerMutation.isPending}
        submittingLabel="Joining..."
      >
        <div className="flex flex-col gap-2">
          <Label htmlFor="join-name">Your name</Label>
          <Input
            id="join-name"
            value={joinName}
            onChange={(e) => setJoinName(e.target.value)}
            placeholder="Enter your name"
          />
        </div>
      </CustomDialog>

      <CustomDialog
        open={fullLobbySession !== null}
        onOpenChange={(open) => !open && setFullLobbySession(null)}
        title="Lobby is full"
        description={
          fullLobbySession
            ? `${fullLobbySession.name} already has ${fullLobbySession.maxPlayers} players. Please choose another lobby or create a new one.`
            : "This lobby is already full."
        }
        onConfirm={() => setFullLobbySession(null)}
        confirmLabel="OK"
      >
        <></>
      </CustomDialog>
    </div>
  );


}
