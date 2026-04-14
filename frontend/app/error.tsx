"use client";

import { useEffect } from "react";

interface ErrorProps {
  error: Error;
  reset: () => void;
}

export default function GlobalError({ error, reset }: ErrorProps) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <main style={{ padding: "2rem", fontFamily: "Inter, system-ui, sans-serif" }}>
      <h1 style={{ marginBottom: "1rem" }}>Something went wrong</h1>
      <p style={{ marginBottom: "1rem", color: "#7f1d1d" }}>
        {error.message || "An error occurred while rendering the page."}
      </p>
      <button
        style={{
          borderRadius: "9999px",
          background: "#4338ca",
          color: "white",
          border: "none",
          padding: "0.75rem 1.25rem",
          cursor: "pointer",
        }}
        onClick={() => reset()}
      >
        Try again
      </button>
    </main>
  );
}
