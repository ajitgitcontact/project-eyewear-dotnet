"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/context/AuthContext";
import { AuthClient } from "@/lib/api/authClient";
import styles from "@/styles/auth.module.css";

type AuthMode = "login" | "signup";

const initialSignUpState = {
  firstName: "",
  lastName: "",
  email: "",
  contactNumber: "",
  password: "",
};

export default function LoginForm() {
  const router = useRouter();
  const { login, isLoading, error } = useAuth();
  const [mode, setMode] = useState<AuthMode>("login");
  const [email, setEmail] = useState("admin@eyewear.com");
  const [password, setPassword] = useState("");
  const [localError, setLocalError] = useState("");
  const [localSuccess, setLocalSuccess] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [signUpData, setSignUpData] = useState(initialSignUpState);

  const busy = isLoading || isSubmitting;

  const switchMode = (nextMode: AuthMode) => {
    setMode(nextMode);
    setLocalError("");
    setLocalSuccess("");
  };

  const handleLoginSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError("");
    setLocalSuccess("");

    if (!email || !password) {
      setLocalError("Email and password are required");
      return;
    }

    try {
      await login(email, password);
      router.push("/");
    } catch (err) {
      setLocalError(err instanceof Error ? err.message : "Login failed");
    }
  };

  const handleSignUpSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLocalError("");
    setLocalSuccess("");

    if (!signUpData.firstName.trim() || !signUpData.lastName.trim()) {
      setLocalError("First name and last name are required");
      return;
    }

    if (!signUpData.email.trim() || !signUpData.password) {
      setLocalError("Email and password are required");
      return;
    }

    if (signUpData.password.length < 8) {
      setLocalError("Password must be at least 8 characters long");
      return;
    }

    try {
      setIsSubmitting(true);

      await AuthClient.signUp({
        firstName: signUpData.firstName.trim(),
        lastName: signUpData.lastName.trim(),
        email: signUpData.email.trim(),
        contactNumber: signUpData.contactNumber.trim() || undefined,
        password: signUpData.password,
      });

      await login(signUpData.email.trim(), signUpData.password);
      setLocalSuccess("Account created successfully.");
      router.push("/");
    } catch (err) {
      setLocalError(err instanceof Error ? err.message : "Signup failed");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className={styles.container}>
      <div className={styles.card}>
        <div className={styles.modeSwitch}>
          <button
            type="button"
            onClick={() => switchMode("login")}
            className={`${styles.modeButton} ${mode === "login" ? styles.modeButtonActive : ""}`}
          >
            Login
          </button>
          <button
            type="button"
            onClick={() => switchMode("signup")}
            className={`${styles.modeButton} ${mode === "signup" ? styles.modeButtonActive : ""}`}
          >
            Sign Up
          </button>
        </div>

        <h1 className={styles.title}>{mode === "login" ? "Eyewear Login" : "Create Account"}</h1>

        {mode === "login" ? (
          <form onSubmit={handleLoginSubmit} className={styles.form}>
            <div className={styles.formGroup}>
              <label htmlFor="email" className={styles.label}>
                Email
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className={styles.input}
                disabled={busy}
                required
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="password" className={styles.label}>
                Password
              </label>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Enter your password"
                className={styles.input}
                disabled={busy}
                required
              />
            </div>

            {(error || localError) && <div className={styles.error}>{error || localError}</div>}

            <button type="submit" className={styles.button} disabled={busy}>
              {busy ? "Logging in..." : "Login"}
            </button>
          </form>
        ) : (
          <form onSubmit={handleSignUpSubmit} className={styles.form}>
            <div className={styles.formRow}>
              <div className={styles.formGroup}>
                <label htmlFor="firstName" className={styles.label}>
                  First Name
                </label>
                <input
                  id="firstName"
                  type="text"
                  value={signUpData.firstName}
                  onChange={(e) => setSignUpData((prev) => ({ ...prev, firstName: e.target.value }))}
                  className={styles.input}
                  disabled={busy}
                  required
                />
              </div>

              <div className={styles.formGroup}>
                <label htmlFor="lastName" className={styles.label}>
                  Last Name
                </label>
                <input
                  id="lastName"
                  type="text"
                  value={signUpData.lastName}
                  onChange={(e) => setSignUpData((prev) => ({ ...prev, lastName: e.target.value }))}
                  className={styles.input}
                  disabled={busy}
                  required
                />
              </div>
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="signupEmail" className={styles.label}>
                Email
              </label>
              <input
                id="signupEmail"
                type="email"
                value={signUpData.email}
                onChange={(e) => setSignUpData((prev) => ({ ...prev, email: e.target.value }))}
                className={styles.input}
                disabled={busy}
                required
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="contactNumber" className={styles.label}>
                Contact Number
              </label>
              <input
                id="contactNumber"
                type="tel"
                value={signUpData.contactNumber}
                onChange={(e) => setSignUpData((prev) => ({ ...prev, contactNumber: e.target.value }))}
                className={styles.input}
                disabled={busy}
                placeholder="Optional"
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="signupPassword" className={styles.label}>
                Password
              </label>
              <input
                id="signupPassword"
                type="password"
                value={signUpData.password}
                onChange={(e) => setSignUpData((prev) => ({ ...prev, password: e.target.value }))}
                className={styles.input}
                disabled={busy}
                placeholder="Minimum 8 characters"
                required
              />
            </div>

            {(error || localError) && <div className={styles.error}>{error || localError}</div>}
            {localSuccess && <div className={styles.success}>{localSuccess}</div>}

            <button type="submit" className={styles.button} disabled={busy}>
              {busy ? "Creating account..." : "Create Account"}
            </button>
          </form>
        )}

        <p className={styles.hint}>
          {mode === "login"
            ? "Demo: admin@eyewear.com"
            : "Signup creates a CUSTOMER account using the backend auth API."}
        </p>
      </div>
    </div>
  );
}
