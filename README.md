Installation instructions:
https://www.rabbitmq.com/install-homebrew.html

ALWAYS IN THE BACKGROUND
brew services start rabbitmq

IN THE FOREGROUND
cd /usr/local/sbin
./rabbitmq-server 

http://localhost:15672/

dotnet build
dotnet run

rabbit commands:

See virtual hosts:
curl -i -u guest:guest http://localhost:15672/api/vhosts

Create virtual host:
curl -i -u guest:guest -H "content-type:application/json" -XPUT http://localhost:15672/api/vhosts/newhost

