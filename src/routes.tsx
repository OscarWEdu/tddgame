import type { RouteObject } from "react-router-dom";
import GamePage from "./pages/GamePage.tsx";
import HomePage from "./pages/HomePage.tsx";
import LobbyPage from "./pages/LobbyPage.tsx";

const routes: RouteObject[] = [
  { path: "/", element: <HomePage /> },
  { path: "/game/:sessionId", element: <GamePage /> },
  { path: "/lobby/:sessionId", element: <LobbyPage /> },
];

export default routes;
