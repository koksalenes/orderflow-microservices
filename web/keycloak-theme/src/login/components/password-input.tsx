import { useState, type ComponentProps } from "react";
import { Eye, EyeOff } from "lucide-react";
import { Input } from "@/components/ui/input";

export function PasswordInput({
    showLabel,
    hideLabel,
    ...props
}: Omit<ComponentProps<"input">, "type"> & { showLabel: string; hideLabel: string }) {
    const [isRevealed, setIsRevealed] = useState(false);

    return (
        <div className="relative">
            <Input {...props} type={isRevealed ? "text" : "password"} className="pr-10" />
            <button
                type="button"
                onClick={() => setIsRevealed(value => !value)}
                aria-label={isRevealed ? hideLabel : showLabel}
                aria-controls={props.id}
                className="absolute inset-y-0 right-1.5 my-auto inline-flex size-7 cursor-pointer items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
                {isRevealed ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
            </button>
        </div>
    );
}
