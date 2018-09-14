FROM microsoft/dotnet:2.1-runtime-alpine
WORKDIR /twitter-bot

COPY ./out ./bin
COPY ./TwitterBot/Config.ini ./

# Copy and build everything else
RUN apk update && \
    apk add bash && \
    echo "#!/bin/bash" >> run && \
    echo "dotnet bin/TwitterBot.dll &" >> run && \ 
    echo "/bin/bash" >> run && \ 
    chmod u+x run

CMD ["./run"]
