import path from "node:path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import { tanstackRouter } from "@tanstack/router-plugin/vite";

const bff = process.env.BFF_URL ?? "http://localhost:5111";
const bffPaths = ["/bff", "/api", "/signin-oidc", "/signout-callback-oidc"];

export default defineConfig({
    plugins: [tanstackRouter({ target: "react", autoCodeSplitting: true }), react(), tailwindcss()],
    resolve: {
        alias: {
            "@": path.resolve(import.meta.dirname, "./src")
        }
    },
    server: {
        port: 3000,
        strictPort: true,
        proxy: Object.fromEntries(bffPaths.map(p => [p, { target: bff, changeOrigin: false, xfwd: true }]))
    }
});
