import { defineConfig } from "orval";

export default defineConfig({
  tddgame: {
    input: {
      target: "http://localhost:5001/swagger/v1/swagger.json",
    },
    output: {
      target: "./src/api/generated/",
      schemas: "./src/api/generated/models",
      mode: "tags-split",
      client: "react-query",
    },
  },
});
