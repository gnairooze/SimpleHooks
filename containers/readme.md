# simple-hooks

Simple-Hooks is a solution to empower your app with events, and let other APIs to subscribe to various events.
It posts the event data to every subscribed API.

All you have to do in your app is to trigger the event you want, by calling Simple-Hooks API.

Simple-Hooks will call all the subscribers, with the event data and retry on them in case of failure.


you can find the source code in the [SimpleHooks](https://github.com/gnairooze/SimpleHooks) repository.

## Getting Started

### Prerequisites

this image depends on SQL server, which should have 2 databases:

- SimpleHooks_Log_DB: for logs. it is created by executing the scripts in [github log-db](https://github.com/gnairooze/SimpleHooks/tree/main/code/SQL/log-db). for more details please check the [readme file](https://github.com/gnairooze/SimpleHooks/blob/main/README.md).
- SimpleHooks: the main operation db. it is created by executing the scripts in [github main-db](https://github.com/gnairooze/SimpleHooks/tree/main/code/SQL/operation-db). for more details please check the [readme file](https://github.com/gnairooze/SimpleHooks/blob/main/README.md).

both should be created before running the container. 

### using docker compose

sample docker compose file and production.env file and related files exist in [github containers](https://github.com/gnairooze/SimpleHooks/tree/main/containers).

the docker-compose.yml file contains 5 services:
1. simple-hooks-web-1: anonymous simple-hooks web api
2. simple-hooks-authapi-1: authenticated simple-hooks web api
3. simple-hooks-processor-1: simple-hooks server processor
4. identity-api-instance-1: identity server api using openiddict
5. simple-hooks-sample-listener-1: a sample listener api to test the solution

all the services needs sql server databases.
run the following command in the directory containing the docker-compose.yml file:

```bash
docker-compose up -d
```

