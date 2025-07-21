/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    extend: {
      colors: {
        // Retro-futuristic cyberpunk color palette
        cyber: {
          dark: "#0a0a0f",
          darker: "#050507",
          blue: "#00f5ff",
          purple: "#9d4edd",
          pink: "#ff10f0",
          green: "#39ff14",
          orange: "#ff6600",
          gray: "#1a1a2e",
          "light-gray": "#16213e",
          accent: "#0066ff",
        },
        // Neon glow colors
        neon: {
          cyan: "#00ffff",
          magenta: "#ff00ff",
          green: "#00ff00",
          orange: "#ffaa00",
          purple: "#aa00ff",
          blue: "#0088ff",
        },
      },
      fontFamily: {
        cyber: ["Orbitron", "monospace"],
        mono: ["Fira Code", "Courier New", "monospace"],
      },
      animation: {
        "fade-in": "fadeIn 0.5s ease-in-out",
        "slide-in": "slideIn 0.3s ease-out",
        "glow-pulse": "glowPulse 2s ease-in-out infinite alternate",
        "scan-line": "scanLine 2s linear infinite",
        "data-flow": "dataFlow 3s linear infinite",
        hologram: "hologram 4s ease-in-out infinite",
        flicker: "flicker 0.15s infinite linear",
      },
      keyframes: {
        fadeIn: {
          "0%": { opacity: "0" },
          "100%": { opacity: "1" },
        },
        slideIn: {
          "0%": { transform: "translateY(-10px)", opacity: "0" },
          "100%": { transform: "translateY(0)", opacity: "1" },
        },
        glowPulse: {
          "0%": {
            boxShadow: "0 0 5px #00ffff, 0 0 10px #00ffff, 0 0 15px #00ffff",
          },
          "100%": {
            boxShadow: "0 0 10px #00ffff, 0 0 20px #00ffff, 0 0 30px #00ffff",
          },
        },
        scanLine: {
          "0%": { transform: "translateX(-100%)" },
          "100%": { transform: "translateX(100vw)" },
        },
        dataFlow: {
          "0%": { transform: "translateX(-100%) scale(0.8)", opacity: "0" },
          "50%": { opacity: "1" },
          "100%": { transform: "translateX(100%) scale(1.2)", opacity: "0" },
        },
        hologram: {
          "0%, 100%": { opacity: "1", transform: "skew(0deg)" },
          "25%": { opacity: "0.8", transform: "skew(0.5deg)" },
          "50%": { opacity: "1", transform: "skew(-0.5deg)" },
          "75%": { opacity: "0.9", transform: "skew(0.3deg)" },
        },
        flicker: {
          "0%, 100%": { opacity: "1" },
          "50%": { opacity: "0.8" },
        },
      },
      backdropBlur: {
        xs: "2px",
      },
      boxShadow: {
        "neon-sm": "0 0 5px currentColor",
        "neon-md": "0 0 10px currentColor, 0 0 20px currentColor",
        "neon-lg":
          "0 0 15px currentColor, 0 0 30px currentColor, 0 0 45px currentColor",
        cyber: "0 8px 32px rgba(0, 255, 255, 0.3)",
        "inner-glow": "inset 0 0 10px rgba(0, 255, 255, 0.2)",
      },
      backgroundImage: {
        "grid-pattern":
          "linear-gradient(rgba(0,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(0,255,255,0.1) 1px, transparent 1px)",
        "cyber-gradient":
          "linear-gradient(135deg, #0a0a0f 0%, #1a1a2e 50%, #16213e 100%)",
        hologram:
          "linear-gradient(45deg, transparent 30%, rgba(0,255,255,0.1) 50%, transparent 70%)",
      },
      backgroundSize: {
        grid: "20px 20px",
      },
    },
  },
  plugins: [],
}
