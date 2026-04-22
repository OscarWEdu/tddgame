import { useParams } from "react-router-dom";


GamePage.route = {
  path: "/game/:sessionId",
};



export default function GamePage() {
const { sessionId } = useParams<{ sessionId: string }>();

  return (
    <div className="container mx-auto">
      <h1>Game</h1>
      <p className="mt-2 text-white/70">Session id: {sessionId}</p>
    </div>
  );
}
