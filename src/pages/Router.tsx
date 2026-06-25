import { createBrowserRouter } from "react-router-dom";

import { Layout } from "@/Layout";
import { HomePage } from "./HomePage";
import { ChatPage } from "./ChatPage";
import { NotFoundPage } from "./NotFoundPage";

export const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      {
        path: "/",
        element: <HomePage />,
      },
      {
        path: "/chat",
        element: <ChatPage />,
      },
      {
        path: "*",
        element: <NotFoundPage />,
      },
    ],
  },
]);
