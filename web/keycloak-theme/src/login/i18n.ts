import { i18nBuilder } from "keycloakify/login";
import type { ThemeName } from "../kc.gen";

const { useI18n, ofTypeI18n } = i18nBuilder
    .withThemeName<ThemeName>()
    .withCustomTranslations({
        en: {
            brandHeadline: "Everything you order, in one flow.",
            brandDescription: "Browse the catalog, pay securely, and follow every order from checkout to your door.",
            brandFeatureTrackingTitle: "Live order tracking",
            brandFeatureTrackingText: "See every step of your order the moment it happens.",
            brandFeatureCheckoutTitle: "Secure checkout",
            brandFeatureCheckoutText: "Your account and payments are protected end to end.",
            brandFeatureNotificationsTitle: "Instant updates",
            brandFeatureNotificationsText: "Get notified by email and in the app when something changes.",
            loginAccountTitle: "Sign in to your account",
            loginAccountSubtitle: "Welcome back! Enter your details to continue.",
            doLogIn: "Sign in",
            doForgotPassword: "Forgot password?",
            noAccount: "New to OrderFlow?",
            doRegister: "Create account",
            registerTitle: "Create your account",
            registerSubtitle: "It only takes a minute. Track your orders and check out faster.",
            alreadyHaveAccount: "Already have an account?",
            registerHint: "We'll send a verification link to your email.",
            passwordConfirm: "Confirm password",
            emailForgotTitle: "Forgot your password?",
            emailInstruction: "No worries. Enter the email you signed up with and we'll send you a reset link.",
            sendResetLink: "Send reset link",
            backToLogin: "Back to sign in",
            emailPlaceholder: "you@example.com",
            email: "Email",
            password: "Password",
            legalNotice: "By continuing, you agree to OrderFlow's Terms of Service and Privacy Policy.",
            copyright: "© {0} OrderFlow. All rights reserved."
        },
        tr: {
            brandHeadline: "Siparişlerinin tamamı, tek bir akışta.",
            brandDescription: "Kataloğu keşfet, güvenle öde ve her siparişini ödemeden kapına kadar takip et.",
            brandFeatureTrackingTitle: "Canlı sipariş takibi",
            brandFeatureTrackingText: "Siparişinin her adımını gerçekleştiği anda gör.",
            brandFeatureCheckoutTitle: "Güvenli ödeme",
            brandFeatureCheckoutText: "Hesabın ve ödemelerin uçtan uca korunur.",
            brandFeatureNotificationsTitle: "Anlık bildirimler",
            brandFeatureNotificationsText: "Bir şey değiştiğinde e-posta ve uygulama üzerinden haberdar ol.",
            loginAccountTitle: "Hesabına giriş yap",
            loginAccountSubtitle: "Tekrar hoş geldin! Devam etmek için bilgilerini gir.",
            doLogIn: "Giriş yap",
            doForgotPassword: "Şifremi unuttum",
            noAccount: "OrderFlow'da yeni misin?",
            doRegister: "Hesap oluştur",
            registerTitle: "Hesabını oluştur",
            registerSubtitle: "Sadece bir dakika sürer. Siparişlerini takip et, daha hızlı alışveriş yap.",
            alreadyHaveAccount: "Zaten hesabın var mı?",
            registerHint: "E-posta adresine bir doğrulama bağlantısı göndereceğiz.",
            passwordConfirm: "Şifre (tekrar)",
            emailForgotTitle: "Şifreni mi unuttun?",
            emailInstruction: "Sorun değil. Kayıt olduğun e-posta adresini gir, sana bir sıfırlama bağlantısı gönderelim.",
            sendResetLink: "Sıfırlama bağlantısı gönder",
            backToLogin: "Girişe dön",
            emailPlaceholder: "ornek@eposta.com",
            email: "E-posta",
            password: "Şifre",
            legalNotice: "Devam ederek OrderFlow Kullanım Koşulları'nı ve Gizlilik Politikası'nı kabul etmiş olursun.",
            copyright: "© {0} OrderFlow. Tüm hakları saklıdır."
        }
    })
    .build();

type I18n = typeof ofTypeI18n;

export { useI18n, type I18n };
