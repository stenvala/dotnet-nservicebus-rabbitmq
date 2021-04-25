# Basic nServiceBus examples with RabbitMQ

This repository includes two basic nServiceBus examples. 
* HelloWorld folder includes an example in which user can send two different messages to the same queue and there are handlers for both of these.
* Saga folder includes an example of using Saga to monitor when a batch operation has finished. User executes in a batch some file processing operation and once all files are processed, the results are zipped by batch finalizer.

## Install and setup RabbitMQ (in macOS)

Installation instructions:
https://www.rabbitmq.com/install-homebrew.html

Run as a service in the background
```
brew services start rabbitmq
```

Run in the 
```
cd /usr/local/sbin
./rabbitmq-server 
```

Access web api
```
http://localhost:15672/
```

Username: guest
Password: guest

Create queues with name: MyFirstQueue and MyFileSagaFinished to Virtual host /MyFirstHost from browser or terminal with commands below

```
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/vhosts/MyFirstHost
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/queues/MyFirstHost/MyFirstQueue
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/queues/MyFirstHost/MyFileSagaFinisher
```

## HelloWorld

Build the solution and run

```
cd HelloWorld
dotnet build
dotnet run
```

See terminal how to send messages.

# Sage

Build and run like Hello World

Read about [nServiceBus sagas](https://docs.particular.net/tutorials/nservicebus-sagas/).
