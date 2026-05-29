import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";
export default defineConfig(function (_a) {
    var mode = _a.mode;
    var env = loadEnv(mode, process.cwd(), "");
    var apiProxyTarget = env.VITE_API_PROXY_TARGET || "http://127.0.0.1:8080";
    return {
        plugins: [react()],
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
            },
        },
        server: {
            host: "0.0.0.0",
            port: 5173,
            proxy: {
                "/api": {
                    target: apiProxyTarget,
                    changeOrigin: true,
                },
            },
        },
    };
});
