import type Route from './interfaces/Route.ts';
import { createElement } from 'react';

// page components
import HomePage from './pages/HomePage.tsx';
import LobbyPage from './pages/LobbyPage.tsx';
import GamePage from './pages/GamePage.tsx';

export default [
  HomePage,
  LobbyPage,
  GamePage
]
  // map the route property of each page component to a Route
  .map((x) => ({ element: createElement(x), ...x.route }) as Route)
  // sort by index (and if an item has no index, sort as index 0)
  .sort((a, b) => (a.index || 0) - (b.index || 0));
