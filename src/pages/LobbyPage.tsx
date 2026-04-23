import { Button } from "@/components/ui/button";
import { useNavigate, useParams } from "react-router-dom";

import { GameSessionStatus } from "@/api/generated/models";
import { useGetApiGameSessionId, usePatchApiGameSessionIdStatus } from "../api/generated/game-sessions/game-sessions";
import { useEffect } from "react";
import { toast } from "sonner";

LobbyPage.route = {
  path: "/lobby/:sessionId",
};

export default function LobbyPage() {
  const navigate = useNavigate();
  const { sessionId } = useParams();

  const { mutateAsync } = usePatchApiGameSessionIdStatus()

  //Fetches session status every x/1000 sec
  const { data, isError } = useGetApiGameSessionId(sessionId!, {
    query: {
      refetchInterval: 2000
    }
  });
  if(isError){
    toast.error("Error fetching session");
  }

  //Navigates to the game page if the session has started
  useEffect(() => {
  if (!data || !data.data) return;
  const session = data.data;
  if (typeof session === "string") return; // If the backend returned a string, ignore it
  if (session.status === GameSessionStatus.started) {
    navigate(`/game/${sessionId!}`);
  }
}, [data, navigate, sessionId]);

  //Function to navigate to the game and handle all the setup
  const handleStart = async () => {
    try {
      await mutateAsync({
        id: sessionId!,
        data: {
          status: GameSessionStatus.started // <-- must match UpdateGameSessionStatusRequest
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
