import { ImageOff } from "lucide-react";
import { useState } from "react";
import { cn } from "@/lib/utils";

export function ProductImage({ src, alt, className, eager = false }: { src: string | null; alt: string; className?: string; eager?: boolean }) {
    const [failed, setFailed] = useState(false);

    return (
        <div className={cn("flex aspect-square items-center justify-center overflow-hidden bg-white", className)}>
            {src === null || failed ? (
                <ImageOff className="size-8 text-muted-foreground/50" aria-hidden />
            ) : (
                <img
                    src={src}
                    alt={alt}
                    loading={eager ? "eager" : "lazy"}
                    decoding="async"
                    className="size-full object-contain p-3 transition-transform duration-300 group-hover:scale-105"
                    onError={() => setFailed(true)}
                />
            )}
        </div>
    );
}
