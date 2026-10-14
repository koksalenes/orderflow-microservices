import type { ClassKey } from "keycloakify/login";

const input =
    "h-10 w-full min-w-0 rounded-lg border border-input bg-background px-3 py-1 text-base shadow-xs transition-[color,box-shadow,border-color] outline-none hover:border-ring/60 placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 disabled:cursor-not-allowed disabled:opacity-50 aria-invalid:border-destructive aria-invalid:ring-3 aria-invalid:ring-destructive/20 md:text-sm dark:bg-input/30";

const button =
    "inline-flex h-10 shrink-0 cursor-pointer items-center justify-center gap-1.5 rounded-lg border border-transparent px-3 text-sm font-medium whitespace-nowrap transition-all outline-none focus-visible:ring-3 focus-visible:ring-ring/50 disabled:pointer-events-none disabled:opacity-50";

export const classes = {
    kcFormClass: "flex flex-col gap-5",
    kcFormGroupClass: "flex flex-col gap-2",
    kcFormGroupHeader: "text-sm font-medium",
    kcLabelWrapperClass: "flex items-center gap-2",
    kcLabelClass: "text-sm leading-snug font-medium",
    kcInputWrapperClass: "flex flex-col gap-2",
    kcInputClass: input,
    kcTextareaClass: `${input} h-auto min-h-20 py-2`,
    kcInputGroup: "relative flex items-center [&>input]:pr-9",
    kcInputErrorMessageClass: "text-sm text-destructive",
    kcInputHelperTextBeforeClass: "text-sm text-muted-foreground",
    kcInputHelperTextAfterClass: "text-sm text-muted-foreground",
    kcFormPasswordVisibilityButtonClass:
        "absolute right-1.5 inline-flex size-7 cursor-pointer items-center justify-center rounded-md text-muted-foreground hover:text-foreground",
    kcFormPasswordVisibilityIconShow: "kc-icon kc-icon-eye",
    kcFormPasswordVisibilityIconHide: "kc-icon kc-icon-eye-off",
    kcCheckboxInputClass: "size-4 accent-primary",
    kcInputClassCheckbox: "flex items-center gap-2",
    kcInputClassCheckboxInput: "size-4 accent-primary",
    kcInputClassCheckboxLabel: "text-sm",
    kcInputClassRadio: "flex items-center gap-2",
    kcInputClassRadioInput: "size-4 accent-primary",
    kcInputClassRadioLabel: "text-sm",
    kcFormOptionsClass: "text-sm",
    kcFormOptionsWrapperClass:
        "text-center text-sm text-muted-foreground [&_a]:font-medium [&_a]:text-primary [&_a]:underline-offset-4 [&_a:hover]:underline",
    kcFormSettingClass: "",
    kcFormButtonsClass: "flex flex-col gap-2",
    kcFormButtonsWrapperClass: "flex flex-col gap-2",
    kcButtonClass: button,
    kcButtonPrimaryClass: "bg-primary text-primary-foreground shadow-xs hover:bg-blue-700 active:bg-blue-800",
    kcButtonDefaultClass: "border-border bg-background hover:bg-muted dark:border-input dark:bg-input/30",
    kcButtonBlockClass: "w-full",
    kcButtonLargeClass: "h-10",
    kcContentWrapperClass: "flex flex-col gap-1",
    kcSelectAuthListClass: "flex flex-col gap-2",
    kcSelectAuthListItemClass:
        "flex items-center gap-3 rounded-lg border p-3 transition-colors hover:bg-muted",
    kcSelectAuthListItemHeadingClass: "text-sm font-medium",
    kcSelectAuthListItemDescriptionClass: "text-sm text-muted-foreground",
    kcRecoveryCodesList: "grid grid-cols-2 gap-1 rounded-lg bg-muted p-3 font-mono text-sm"
} satisfies { [key in ClassKey]?: string };
