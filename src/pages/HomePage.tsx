import { Button } from "@/components/ui/button";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import {
  useGetApiGameSession,
  useGetApiGameSessionId,
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

export default function HomePage() {
  const { data, isLoading, isError } = useGetApiGameSession();
  const gameSessionMutation = usePostApiGameSession();
  const playerMutation = usePostApiPlayers();
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

  const isCreatingGame =
    gameSessionMutation.isPending || playerMutation.isPending;

  const { data: gameSessionById } = useGetApiGameSessionId(
    "550e8400-e29b-41d4-a716-446655440000",
  );

  console.log("gameSessionById", gameSessionById);

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);
  return (
    <>
      <div className="fixed inset-0 -z-10 bg-[url('/background.jpg')] bg-cover bg-center bg-no-repeat" />
      <div className="fixed inset-0 flex items-center justify-center">
        <div className="w-full max-w-sm rounded-2xl bg-black/60 p-8 shadow-2xl backdrop-blur-md">
          <h1 className="mb-6 text-center text-3xl font-bold text-white">
            TDD Game
          </h1>
          {view === "menu" && (
            <nav className="flex flex-col gap-3">
              <button
                className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20"
                onClick={() => setView("new-game")}
              >
                Start a new game
              </button>
              <button className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20">
                Load game
              </button>
              <a
                href="https://github.com/OscarWEdu/tddgame/wiki/Standard-RISK-Rules"
                className="rounded-lg bg-white/10 px-4 py-3 text-white text-center transition hover:bg-white/20"
              >
                How to play
              </a>
            </nav>
          )}

          {view === "new-game" && (
            <nav className="flex flex-col gap-3">
              <label>Name your game</label>
              <input
                id="game-name"
                type="text"
                value={gameName}
                onChange={(e) => setGameName(e.target.value)}
                className="rounded-lg bg-white/10 px-4 py-3 text-white placeholder-white/40 outline-none focus:bg-white/20 placeholder:text-sm"
                placeholder="My new game"
              ></input>
              <label>
                Number of players ({minPlayers}-{maxPlayers})
              </label>
              <input
                id="player-count"
                type="number"
                min={minPlayers}
                max={maxPlayers}
                value={playerCount}
                onChange={(e) =>
                  setPlayerCount(
                    Math.min(
                      maxPlayers,
                      Math.max(
                        minPlayers,
                        Number(e.target.value) || minPlayers,
                      ),
                    ),
                  )
                }
              ></input>
              <button
                className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20"
                disabled={!gameName.trim() || isCreatingGame}
                onClick={handleCreateGame}
              >
                {isCreatingGame ? "Creating game..." : "Create game"}
              </button>
              <button
                className="rounded-lg px-4 py-3 text-sm text-white/60 transition hover:text-white"
                onClick={() => setView("menu")}
              >
                Go back
              </button>
            </nav>
          )}

          {view === "ready" && (
            <div className="flex flex-col gap-3">
              <p className="text-center text-white">
                Game created with {playerCount} players!
              </p>
              <button
                className="rounded-lg bg-white/20 px-4 py-3 text-white transition hover:bg-white/30"
                onClick={() => navigate(`/game/${createdSessionId}`)}
              >
                Start game
              </button>
            </div>
          )}
        </div>
      </div>

      <div className="container mx-auto">
        <h1>Hello World</h1>
        <Button
          className="bg-success"
          onClick={() => {
            toast.success("Let's goooo!");
          }}
        >
          <span>Success</span>
        </Button>
        <Button
          onClick={() => {
            toast.info("Important info!");
          }}
        >
          <span>Info</span>
        </Button>
        <Button
          className="bg-warning"
          onClick={() => {
            toast.warning("Oh Warning!");
          }}
        >
          <span>Warning</span>
        </Button>
        <Button
          className="bg-destructive"
          onClick={() => {
            toast.error("Oh nooooo!");
          }}
        >
          <span>Error</span>
        </Button>
      </div>
    </>
  );
}
