import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import {
  useGetApiGameSession,
  usePostApiGameSession,
} from "../api/generated/game-sessions/game-sessions";
import { usePostApiPlayers } from "../api/generated/players/players";

HomePage.route = {
  path: "/",
};

type View = "menu" | "new-game" | "load-game" | "ready";

const defaultColours = ["Black", "Blue", "Green", "Pink", "Red", "Yellow"];
const minPlayers = 2;
const maxPlayers = 6;

const glassBtn =
  "w-full rounded-lg bg-secondary/50 px-4 py-3 text-foreground transition hover:bg-secondary h-auto justify-start font-normal";

export default function HomePage() {
  const { data, isLoading, isError, error } = useGetApiGameSession();
  const gameSessionMutation = usePostApiGameSession();
  const playerMutation = usePostApiPlayers({
    mutation: {
      onError: () => {
        toast.error("");
      },
    },
  });
  const navigate = useNavigate();
  const [view, setView] = useState<View>("menu");
  const [gameName, setGameName] = useState("");
  const [playerCount, setPlayerCount] = useState(minPlayers);
  const [createdSessionId, setCreatedSessionId] = useState<string | null>(null);

  const handleCreateGame = async () => {
    const session = await gameSessionMutation.mutateAsync({
      data: { name: gameName },
    });
    if (session.status !== 201) {
      console.error("Failed to create game session", session.data);
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

    setCreatedSessionId(sessionId);
    setView("ready");
  };

  if (isError) {
    toast.error("Error!");
  }

  const isCreatingGame =
    gameSessionMutation.isPending || playerMutation.isPending;

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);

  return (
    <div className="fixed inset-0 flex items-center justify-center pointer-events-none">
      <div className="w-full max-w-sm rounded-2xl bg-card/80 p-8 shadow-2xl backdrop-blur-md pointer-events-auto">
        <h1 className="mb-6 text-center text-3xl font-bold text-foreground">
          TDD Game
        </h1>

        {view === "menu" && (
          <nav className="flex flex-col gap-3">
            <Button className={glassBtn} onClick={() => setView("new-game")}>
              Start a new game
            </Button>
            <Button className={glassBtn}>Load game</Button>
          </nav>
        )}

        {view === "new-game" && (
          <div className="flex flex-col gap-3">
            <Label htmlFor="game-name" className="text-foreground">
              Name your game
            </Label>
            <Input
              id="game-name"
              type="text"
              value={gameName}
              onChange={(e) => setGameName(e.target.value)}
              className="rounded-lg bg-secondary/50 border-border px-4 py-3 text-foreground placeholder:text-muted-foreground placeholder:text-sm focus-visible:ring-0 focus-visible:bg-secondary"
              placeholder="My new game"
            />
            <Label htmlFor="player-count" className="text-foreground">
              Number of players
            </Label>
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
            <Button
              className={glassBtn}
              disabled={!gameName.trim() || isCreatingGame}
              onClick={handleCreateGame}
            >
              {isCreatingGame ? "Creating game..." : "Create game"}
            </Button>
            <Button
              variant="ghost"
              className="text-muted-foreground hover:text-foreground hover:bg-transparent text-sm"
              onClick={() => setView("menu")}
            >
              Go back
            </Button>
          </div>
        )}

        {view === "ready" && (
          <div className="flex flex-col gap-3">
            <p className="text-center text-foreground">
              Game created with {playerCount} players!
            </p>
            <Button
              className="rounded-lg bg-secondary text-foreground hover:bg-secondary/80 h-auto"
              onClick={() => navigate(`/game/${createdSessionId}`)}
            >
              Start game
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
