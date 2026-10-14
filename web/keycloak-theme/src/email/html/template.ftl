<#--
  OrderFlow email layout: table-based with inline styles so it renders the same in Gmail, Outlook, and Apple Mail.
  Every HTML email of the realm uses it, including the base templates this theme does not override.
-->
<#macro emailLayout preheader="">
<!doctype html>
<html lang="${locale.language}" dir="${(ltr)?then('ltr','rtl')}">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="color-scheme" content="light">
    <meta name="supported-color-schemes" content="light">
    <title>${realmName}</title>
    <style>
        body { margin: 0; padding: 0; background-color: #eff6ff; }
        a { color: #2563eb; }
        p { margin: 0 0 16px 0; }
        @media only screen and (max-width: 620px) {
            .container { width: 100% !important; }
            .content { padding: 28px 24px !important; }
        }
    </style>
</head>
<body style="margin:0;padding:0;background-color:#eff6ff;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,Helvetica,Arial,sans-serif;color:#0f172a;">
    <#if preheader?has_content>
        <div style="display:none;max-height:0;overflow:hidden;opacity:0;">${preheader}</div>
    </#if>
    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#eff6ff;">
        <tr>
            <td align="center" style="padding:40px 16px;">
                <table role="presentation" class="container" width="560" cellpadding="0" cellspacing="0" border="0" style="width:560px;max-width:560px;">
                    <tr>
                        <td align="center" style="padding-bottom:24px;">
                            <table role="presentation" cellpadding="0" cellspacing="0" border="0">
                                <tr>
                                    <td style="background-color:#2563eb;border-radius:8px;width:32px;height:32px;text-align:center;vertical-align:middle;color:#ffffff;font-size:16px;font-weight:700;">O</td>
                                    <td style="padding-left:10px;font-size:18px;font-weight:700;color:#0f172a;">${realmName}</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td style="background-color:#ffffff;border:1px solid #dbeafe;border-radius:14px;overflow:hidden;">
                            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
                                <tr><td style="height:4px;background-color:#2563eb;background-image:linear-gradient(90deg,#1d4ed8,#3b82f6,#60a5fa);font-size:0;line-height:0;">&nbsp;</td></tr>
                                <tr>
                                    <td class="content" style="padding:36px 40px;font-size:15px;line-height:24px;color:#334155;">
                                        <#nested>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                    <tr>
                        <td align="center" style="padding:24px 16px 0 16px;font-size:12px;line-height:18px;color:#64748b;">
                            ${msg("orderflowFooter", realmName)}<br>
                            &copy; ${.now?string("yyyy")} ${realmName}
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>
</#macro>

<#macro heading text>
    <h1 style="margin:0 0 16px 0;font-size:22px;line-height:30px;font-weight:700;color:#0f172a;">${text}</h1>
</#macro>

<#macro greeting>
    <#if user?? && user.firstName?has_content>
        <p style="margin:0 0 16px 0;">${msg("orderflowGreeting", user.firstName)}</p>
    </#if>
</#macro>

<#macro button href label>
    <table role="presentation" cellpadding="0" cellspacing="0" border="0" style="margin:8px 0 24px 0;">
        <tr>
            <td style="border-radius:8px;background-color:#2563eb;">
                <a href="${href}" target="_blank" style="display:inline-block;padding:12px 24px;font-size:15px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;">${label}</a>
            </td>
        </tr>
    </table>
</#macro>

<#macro linkFooter href expiration="">
    <#if expiration?has_content>
        <p style="margin:0 0 16px 0;font-size:13px;color:#64748b;">${msg("orderflowLinkExpiry", expiration)}</p>
    </#if>
    <p style="margin:0 0 8px 0;font-size:13px;color:#64748b;">${msg("orderflowFallbackLink")}</p>
    <p style="margin:0 0 16px 0;font-size:11px;line-height:16px;word-break:break-all;"><a href="${href}" style="color:#94a3b8;">${href}</a></p>
    <p style="margin:0;padding-top:16px;border-top:1px solid #e2e8f0;font-size:13px;color:#64748b;">${msg("orderflowIgnoreNotice")}</p>
</#macro>
