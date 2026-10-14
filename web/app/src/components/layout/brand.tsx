import { Link } from "@tanstack/react-router";
import { Workflow } from "lucide-react";
import { cn } from "@/lib/utils";

export function Brand({ className }: { className?: string }) {
    return (
        <Link to="/" className={cn("flex items-center gap-2.5 text-lg font-semibold tracking-tight", className)}>
            <span className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
                <Workflow className="size-5" />
            </span>
            OrderFlow
        </Link>
    );
}
