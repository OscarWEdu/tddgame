import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import type { RouteObject } from "react-router-dom";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import App from "./App";
import { Toaster } from "./components/ui/sonner";
import routes from "./routes";
import { AuthProvider } from "./utils/AuthProvider";

// Create a router using settings/content from 'routes.tsx'
const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: routes as RouteObject[],
    HydrateFallback: App,
  },
]);

const queryClient = new QueryClient();

// Create the React root element
createRoot(document.querySelector("#root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
    <Toaster
      toastOptions={{
        unstyled: false,
      }}
    />
  </StrictMode>,
);
