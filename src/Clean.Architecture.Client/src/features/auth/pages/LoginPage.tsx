import { Link } from "react-router-dom";
import { LoginForm } from "../components/LoginForm";
import { useAuth } from "../hooks/useAuth";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

export function LoginPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      navigate("/");
    }
  }, [isAuthenticated, navigate]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 sm:px-6 lg:px-8">
      <div className="w-full max-w-md space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-bold tracking-tight text-gray-900">
            Sign in to your account
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Or{" "}
            <Link
              to="/register"
              className="font-medium text-blue-600 hover:text-blue-500"
            >
              create a new account
            </Link>
          </p>
        </div>
        <div className="rounded-lg bg-white px-8 py-8 shadow-md">
          <LoginForm />
        </div>
        <div className="text-center text-sm text-gray-600">
          <p>Demo credentials:</p>
          <p className="mt-1">
            Admin: <span className="font-mono">admin@example.com</span> /{" "}
            <span className="font-mono">Admin123!</span>
          </p>
          <p className="mt-1">
            User: <span className="font-mono">user@example.com</span> /{" "}
            <span className="font-mono">User123!</span>
          </p>
        </div>
      </div>
    </div>
  );
}
