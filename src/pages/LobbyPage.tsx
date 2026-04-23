import { Button } from "@/components/ui/button";
import { useNavigate, useParams } from "react-router-dom";

import { usePatchApiGameSessionIdStatus } from "../api/generated/game-sessions/game-sessions";


LobbyPage.route = {
  path: "/lobby/:sessionId",
};

export default function LobbyPage() {
  const navigate = useNavigate();
  const { sessionId } = useParams();

  const { mutateAsync } = usePatchApiGameSessionIdStatus()

  const handleStart = async () => {
    if (!sessionId) return;

    try {
      await mutateAsync({
        id: sessionId,
        data: {
          status: "started" // <-- must match UpdateGameSessionStatusRequest
        }
      });

      navigate(`/game/${sessionId}`);
    } catch (err) {
      console.error("Failed to update session status", err);
    }
  };

  return (
    <div className="flex flex-col gap-6 py-8 ">
      <div className="text-center text-3xl font-bold mb-6">
        <h1>Start Game</h1>
      </div>
      <div className="flex items-center justify-center">
        <Button onClick={handleStart}>
          Start
        </Button>
      </div>
    </div>
  );
}
