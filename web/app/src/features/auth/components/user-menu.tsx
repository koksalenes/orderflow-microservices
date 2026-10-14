import { Link } from "@tanstack/react-router";
import { LogOut, PackageOpen, UserRound, WalletCards } from "lucide-react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuLabel,
    DropdownMenuSeparator,
    DropdownMenuTrigger
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { useTranslation } from "@/i18n/use-translation";
import { initials } from "@/lib/utils";
import { signIn, signOut } from "../session";
import { useSession } from "../use-session";

export function UserMenu() {
    const { session, isPending } = useSession();
    const { t, locale } = useTranslation();

    if (isPending) {
        return <Skeleton className="h-9 w-24 rounded-lg" />;
    }

    if (session === null) {
        return (
            <div className="flex items-center gap-2">
                <Button variant="ghost" size="lg" className="hidden sm:inline-flex" onClick={() => signIn({ register: true, locale })}>
                    {t("auth.createAccount")}
                </Button>
                <Button size="lg" onClick={() => signIn({ locale })}>
                    {t("auth.signIn")}
                </Button>
            </div>
        );
    }

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="lg" className="gap-2 px-1.5 sm:pr-3">
                    <Avatar className="size-7">
                        <AvatarFallback className="bg-primary/10 text-xs font-semibold text-primary">
                            {initials(session.name)}
                        </AvatarFallback>
                    </Avatar>
                    <span className="hidden max-w-32 truncate text-sm font-medium sm:inline">{session.givenName ?? session.name}</span>
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-60">
                <DropdownMenuLabel className="flex flex-col gap-0.5">
                    <span className="truncate text-sm font-medium">{session.name}</span>
                    <span className="truncate text-xs font-normal text-muted-foreground">{session.email}</span>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                    <Link to="/account">
                        <UserRound /> {t("auth.myAccount")}
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                    <Link to="/wallet" search={{}}>
                        <WalletCards /> {t("wallet.nav")}
                    </Link>
                </DropdownMenuItem>
                <DropdownMenuItem disabled>
                    <PackageOpen /> {t("nav.orders")}
                </DropdownMenuItem>
                <DropdownMenuSeparator />
                <DropdownMenuItem variant="destructive" onSelect={() => signOut(session)}>
                    <LogOut /> {t("auth.signOut")}
                </DropdownMenuItem>
            </DropdownMenuContent>
        </DropdownMenu>
    );
}
