import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
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
  const [gameName, setGameName] = useState("");
  const [playerCount, setPlayerCount] = useState(minPlayers);

  const sessions: GameSessionDto[] = data?.status === 200 ? data.data : [];
  const isCreatingGame =
    gameSessionMutation.isPending || playerMutation.isPending;

  const handleCreateGame = async () => {
    const session = await gameSessionMutation.mutateAsync({
      data: { name: gameName },
    });
    if (session.status !== 201) {
      toast.error("Failed to create game session");
      return;
    }
    const sessionId = session.data.id;

    for (let i = 0; i < playerCount; i++) {
      await playerMutation.mutateAsync({
        data: {
          name: `Player ${i + 1}`,
          colour: defaultColours[i],
          turnOrder: i + 1,
          missionId: 1,
        },
        params: { gameSessionId: sessionId },
      });
    }

    setCreateOpen(false);
    setGameName("");
    setPlayerCount(minPlayers);
    navigate(`/lobby/${sessionId}`);
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
                onClick={() => setJoinSession(session)}
              >
                <CardContent className="flex items-center justify-between px-3 py-2">
                  <span className="text-sm font-medium text-foreground">
                    {session.name}
                  </span>
                  {session.status && (
                    <Badge variant={statusVariant[session.status]}>
                      {statusLabel[session.status]}
                    </Badge>
                  )}
                </CardContent>
              </Card>
            ))}
        </div>
      </ScrollArea>

      <Dialog open={createOpen} onOpenChange={setCreateOpen}>
        <DialogContent className="rounded-md">
          <DialogHeader>
            <DialogTitle>Create a new game</DialogTitle>
            <DialogDescription>
              Set up your game session and choose the number of players.
            </DialogDescription>
          </DialogHeader>
          <div className="flex flex-col gap-4 py-2">
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
              <Label htmlFor="player-count">Number of players</Label>
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
          <DialogFooter>
            <Button
              variant="ghost"
              className="rounded-md"
              onClick={() => setCreateOpen(false)}
            >
              Cancel
            </Button>
            <Button
              className="rounded-md"
              disabled={!gameName.trim() || isCreatingGame}
              onClick={handleCreateGame}
            >
              {isCreatingGame ? "Creating..." : "Create game"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog
        open={joinSession !== null}
        onOpenChange={(open) => !open && setJoinSession(null)}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Join game</DialogTitle>
            <DialogDescription>
              Do you want to join <strong>{joinSession?.name}</strong>?
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="ghost" onClick={() => setJoinSession(null)}>
              Cancel
            </Button>
            <Button onClick={() => navigate(`/lobby/${joinSession?.id}`)}>
              Join
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );


}
