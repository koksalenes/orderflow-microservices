<#outputformat "plainText">
<#assign requiredActionsText><#if requiredActions??><#list requiredActions><#items as reqActionItem>${msg("requiredAction.${reqActionItem}")}<#sep>, </#sep></#items></#list></#if></#assign>
</#outputformat>
<#import "template.ftl" as layout>
<@layout.emailLayout preheader=msg("orderflowExecuteActionsTitle")>
    <@layout.heading text=msg("orderflowExecuteActionsTitle") />
    <@layout.greeting />
    <p style="margin:0 0 16px 0;">${msg("orderflowExecuteActionsIntro", realmName, requiredActionsText)}</p>
    <@layout.button href=link label=msg("orderflowExecuteActionsAction") />
    <@layout.linkFooter href=link expiration=linkExpirationFormatter(linkExpiration) />
</@layout.emailLayout>
