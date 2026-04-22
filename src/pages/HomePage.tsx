
import { useState } from "react";

import {
  useGetApiGameSession,
  usePostApiGameSession,
} from "../api/generated/game-sessions/game-sessions";

HomePage.route = {
  path: "/",
};

type View = "menu" | "new-game" | "load-game";

export default function HomePage() {
  const { data, isLoading, isError } = useGetApiGameSession();
  const gameSessionMutation = usePostApiGameSession();
  const [view, setView] = useState<View>("menu");
  const [gameName, setGameName] = useState("");

  console.log("data", data);
  console.log("isLoading", isLoading);
  console.log("isError", isError);
  return (
    <>
      <div className="fixed inset-0 -z-10 bg-[url('/background.jpg')] bg-cover bg-center bg-no-repeat" />
      <div className="fixed inset-0 flex items-center justify-center">
        <div className="w-full max-w-sm rounded 2xl bg-black/60 p-8 shadow-2xl backdrop-blur-md">
          <h1 className="mb-6 text-center text-3xl font-bold text-white">
            TDD Game
          </h1>
          {view === "menu" &&(
            <nav className="flex flex-col gap-3">
            <button className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20"
            onClick={() => setView("new-game")}
            >
              Start a new game
            </button>
            <button className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20">
              Load game
            </button>
            <a href="https://github.com/OscarWEdu/tddgame/wiki/Standard-RISK-Rules" className="rounded-lg bg-white/10 px-4 py-3 text-white text-center transition hover:bg-white/20">
              How to play
            </a>
          </nav>
          )}

          {view === "new-game" && (
            <nav className="flex flex-col gap-3">
           <label>
            Name your game
           </label>
           <input
           id="game-name" type="text" value={gameName} onChange={(e) => setGameName(e.target.value)} className="rounded-lg bg-white/10 px-4 py-3 text-white placeholder-white/40 outline-none focus:bg-white/20 placeholder:text-sm" placeholder="My new game">
           </input>
            <button className="rounded-lg bg-white/10 px-4 py-3 text-white transition hover:bg-white/20"
            disabled={!gameName.trim()}
            onClick={() => gameSessionMutation.mutate({ data: {name: gameName}})}
            >
            Create game
            </button>
            <button className="rounded-lg px-4 py-3 text-sm text-white/60 transition hover:text-white"
            onClick={() => setView("menu")}
            >
              Go back
            </button>
          </nav>

          )}
          

        </div>

      </div>
      
    </>
  );
}
