import type { Config } from "tailwindcss";

export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        ink: "#11212d",
        paper: "#f5f1e8",
        accent: "#1f6f78",
        signal: "#f28c28",
        maroon: "#5c1f1f",
        "maroon-deep": "#471616",
        "maroon-soft": "#9d5b55",
      },
    },
  },
  plugins: [],
} satisfies Config;
