# Basic nservice bus example

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

Create queue with name: MyFirstQueue to Virtual host /MyFirstHost (see also API commands below to create)

Build the solution

```
cd Service
dotnet build
dotnet run
```

## Rabbit commands

See virtual hosts:
```
curl -i -u guest:guest http://localhost:15672/api/vhosts
```

Create virtual host:
```
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/vhosts/MyFirstHost
```

Create queue to virtual host:
```
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/queues/MyFirstHost/MyFirstQueue
```

# Sage example

Read [nservicebus sagas](https://docs.particular.net/tutorials/nservicebus-sagas/)

For this example, create another queue
```
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/queues/MyFirstHost/MyFileSagaFinisher
```

