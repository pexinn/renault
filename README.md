# MS Renault Lead Integration

Microserviço em .NET 10 para integração de leads entre a Renault e o Salesforce.

## Arquitetura
A solução segue os padrões do Grupo Barigui, dividida em:
- **MsRenault.API**: Ponto de entrada, Webhooks para eventos do Salesforce e Workers de ingestão.
- **MsRenault.Aplicacao**: Serviços de integração e consumo de APIs externas.
- **MsRenault.Dominio**: Modelos de dados (DTOs), Entidades do MongoDB, Interfaces e Mapeamentos.
- **MsRenault.Infra.Dados**: Implementação de Repositórios (MongoDB) e Clientes de API (Renault/Salesforce).
- **MsRenault.Infra.Mensageria**: Implementação de mensageria com RabbitMQ e rastreabilidade OpenTelemetry.

## Funcionalidades
1. **US01**: Autenticação OAuth 2.0 com renovação automática (90 dias).
2. **US02 & US03**: Ingestão periódica de leads da Renault e publicação em fila.
3. **US04**: Consumo de leads da fila, mapeamento e envio para o Salesforce.
4. **US05**: Observabilidade com OpenTelemetry (Traces e Métricas) exportando via OTLP.
5. **Phase 2 & 3**: Atualização de funil, prospecção e envio de leads exclusivos da rede para a Renault.

## Endpoints Internos (API)

### Salesforce Webhook Event
`POST /api/lead/webhook/salesforce/event`
Recebe eventos de mudança de status ou criação de leads no Salesforce para sincronizar com a Renault.

**Body:**
```json
{
  "id": "00Q0...",
  "status": "PROSPECTION",
  "renaultLeadReferenceId": "...",
  "firstName": "João",
  "lastName": "Silva"
}
```

## Integração Renault (Endpoints Consumidos)

### Ingestão de Leads
`POST /v1/leads/consume`
- Captura leads gerados pela montadora.
- Filtro: BIR code, startDate e endDate (UTC).

### Atualização de Funil
`POST /v1/leads/complete/{leadReferenceId}/funnel`
- Sincroniza o status do lead no Salesforce.
- Valores aceitos: `PROSPECTION`, `VISIT`, `TEST_DRIVE`, `NEGOTIATION`.

### Atualização de Prospecção
`POST /v1/leads/complete/{leadReferenceId}/prospection`
- Envia dados do primeiro contato e prospecção.

## Configuração (appsettings.json)
```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017"
  },
  "RabbitMQ": { "HostName": "localhost" },
  "Renault": {
    "BaseUrl": "https://crmi.renault.com.br",
    "AccessKey": "...",
    "Password": "...",
    "Bir": "..."
  }
}
```

## Testes e Validação
### Testes Unitários
Execute `dotnet test` na pasta `src` para rodar a suíte de testes unitários.

### Postman
A collection consolidada está disponível na raiz:
- **`Renault.Integration.postman_collection.json`**: Contém folders organizados para testar sua aplicação localmente (**MsRenault.API (Local)**) e validar credenciais diretamente na montadora (**Renault.API (Direct)**).

Para usar:
1. Importe o JSON no Postman.
2. No folder **MsRenault.API (Local)**, garanta que o microserviço está rodando e a variável `baseUrl` aponta para seu ambiente local (padrão: `http://localhost:5000`).
3. No folder **Renault.API (Direct)**, preencha as variáveis de ambiente (`renaultAccessKey`, `renaultPassword`, etc) para validar o acesso real.

## Observabilidade
Exportação de traces OTLP habilitada por padrão.
Endpoint OTLP: `http://localhost:4317`
Tags customizadas: `renault.bir`, `renault.leadReferenceId`.
