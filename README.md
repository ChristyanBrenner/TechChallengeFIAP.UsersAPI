# TechChallengeFIAP
Projeto desenvolvido como parte do desáfio técnico da pós-graduação da FIAP.

## Sobre o Projeto
Este repositório contém uma aplicação backend desenvolvida em C#, como foco em boas práticas de arquitetura de software

## Tecnologias Utilizadas
- C# / .NET 8
- ASP.NET Core
- Docker e Docker Compose
- Testes com xUnit
- Git e GitHub
- Amazon SQS

## Estrutura do Projeto
- Controllers/ -> Endpoints da API
- Domain/ -> Entidades e regras de negócio
- Middleware/ -> Tratamento de erros, logging
- Repositories/ -> Implementação e persistência de dados
- Services/ -> Lógica de negócio
- Tests/ -> Testes automatizados
- Utils/ -> Funções utilitárias
- Dockerfile -> Configuração do Docker
- docker-compose.yml -> Orquestração com Docker Compose
- CloudGames.API.sln -> Solução do projeto

## Instruções
SUBIR IMAGEM NO DOCKER LOCAL
- docker build --no-cache -t user-api . && docker run -d -p 5000:80 user-api

SUBIR O REPOSITORIO NO AWS App Runner
- aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin 451664151831.dkr.ecr.sa-east-1.amazonaws.com
  
- docker tag user-api:latest 451664151831.dkr.ecr.sa-east-1.amazonaws.com/user-api:latest
  
- docker push 451664151831.dkr.ecr.sa-east-1.amazonaws.com/user-api:latest

## Fluxo de Comunicação
<img width="1000" height="1000" alt="User API Event Processing-2026-03-24-203716" src="https://github.com/user-attachments/assets/f64d7f66-c1e6-4d76-8daa-61ea479381cb" />
