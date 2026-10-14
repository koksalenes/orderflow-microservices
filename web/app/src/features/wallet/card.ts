export type CardBrand = "Visa" | "Mastercard" | "Amex" | "Troy" | "Card";

export type CardForm = { holderName: string; number: string; expiry: string; cvv: string };

export type CardField = keyof CardForm;

export type CardErrorCode =
    | "validation.required"
    | "validation.length"
    | "validation.card_number"
    | "validation.card_expiry"
    | "validation.card_expired"
    | "validation.card_cvv";

export function digitsOnly(value: string) {
    return value.replace(/\D/g, "");
}

export function detectBrand(number: string): CardBrand {
    if (number.startsWith("4")) return "Visa";
    if (/^3[47]/.test(number)) return "Amex";
    if (/^(9792|65)/.test(number)) return "Troy";
    if (/^[25]/.test(number)) return "Mastercard";
    return "Card";
}

export const cvvLength = (number: string) => (detectBrand(number) === "Amex" ? 4 : 3);

export function formatCardNumber(value: string) {
    const digits = digitsOnly(value).slice(0, 19);
    const groups = detectBrand(digits) === "Amex" ? [4, 6, 5] : [4, 4, 4, 4, 3];
    const parts: string[] = [];
    let index = 0;
    for (const size of groups) {
        if (index >= digits.length) break;
        parts.push(digits.slice(index, index + size));
        index += size;
    }
    return parts.join(" ");
}

export function formatExpiry(value: string) {
    const digits = digitsOnly(value).slice(0, 4);
    return digits.length > 2 ? `${digits.slice(0, 2)}/${digits.slice(2)}` : digits;
}

export function parseExpiry(expiry: string) {
    const match = /^(\d{2})\/(\d{2})$/.exec(expiry);
    return match === null ? null : { month: Number(match[1]), year: 2000 + Number(match[2]) };
}

export function validateCard(form: CardForm, now = new Date()): Partial<Record<CardField, CardErrorCode>> {
    const errors: Partial<Record<CardField, CardErrorCode>> = {};
    const number = digitsOnly(form.number);

    const name = form.holderName.trim();
    if (name.length === 0) errors.holderName = "validation.required";
    else if (name.length < 2 || name.length > 100) errors.holderName = "validation.length";

    if (!/^\d{12,19}$/.test(number)) errors.number = "validation.card_number";

    const expiry = parseExpiry(form.expiry);
    if (expiry === null || expiry.month < 1 || expiry.month > 12) {
        errors.expiry = "validation.card_expiry";
    } else if (expiry.year < now.getFullYear() || (expiry.year === now.getFullYear() && expiry.month < now.getMonth() + 1)) {
        errors.expiry = "validation.card_expired";
    }

    if (!new RegExp(`^\\d{${cvvLength(number)}}$`).test(form.cvv)) errors.cvv = "validation.card_cvv";

    return errors;
}

const testExpiry = () => `12/${String((new Date().getFullYear() + 2) % 100).padStart(2, "0")}`;

export function testCard(holderName: string): CardForm {
    return { holderName, number: "4242 4242 4242 4242", expiry: testExpiry(), cvv: "123" };
}

export function failingSettlementTestCard(holderName: string): CardForm {
    return { holderName, number: "4000 0000 0000 0119", expiry: testExpiry(), cvv: "123" };
}

export function fieldOf(serverField: string): CardField | undefined {
    const map: Record<string, CardField> = {
        "card.holderName": "holderName",
        "card.number": "number",
        "card.expiry": "expiry",
        "card.expiryMonth": "expiry",
        "card.expiryYear": "expiry",
        "card.cvv": "cvv"
    };
    return map[serverField];
}
