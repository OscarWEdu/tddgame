import { Button } from "@/components/ui/button";
import { useNavigate, useParams } from "react-router-dom";

export default function LobbyPage() {
  const navigate = useNavigate();
  const { sessionId } = useParams<{ sessionId: string }>();

  return (
    <div className="flex flex-col gap-6 py-8 ">
      <div className="text-center text-3xl font-bold mb-6">
        <h1>Start Game</h1>
      </div>
      <div className="flex items-center justify-center">
        <Button onClick={() => navigate(`/game/${sessionId}`)}>
          Start
        </Button>
      </div>
    </div>
  );
}
