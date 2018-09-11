FROM microsoft/dotnet:2.1-sdk
WORKDIR /twitter-bot

# Copy and build everything else
run git clone https://github.com/zkSNACKs/WasabiStatTwitterBot.git src && \
    cp ./src/TwitterBot/Config.ini . && \
    cd ./src/TwitterBot && dotnet publish -c Release -o ../../bin
 
RUN apt update && \
    echo "deb http://deb.torproject.org/torproject.org stretch main" >> /etc/apt/sources.list && \
    echo "deb-src http://deb.torproject.org/torproject.org stretch main" >> /etc/apt/sources.list && \
    for server in $(shuf -e ha.pool.sks-keyservers.net \
                            hkp://p80.pool.sks-keyservers.net:80 \
                            keyserver.ubuntu.com \
                            hkp://keyserver.ubuntu.com:80 \
                            pgp.mit.edu) ; do \
        gpg --keyserver "$server" --recv A3C4F0F979CAA22CDBA8F512EE8CBC9E886DDD89 && break || : ; \
    done && \
    gpg --export A3C4F0F979CAA22CDBA8F512EE8CBC9E886DDD89 | apt-key add - && \
    apt update -y && apt install -y tor deb.torproject.org-keyring && \
    echo "#!/bin/bash" >> run && \
    echo "/etc/init.d/tor restart" >> run && \ 
    echo "dotnet bin/TwitterBot.dll &" >> run && \ 
    echo "/bin/bash" >> run && \ 
    chmod u+x run

CMD ["./run"]
