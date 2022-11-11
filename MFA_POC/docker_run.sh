docker image rm web_mfa_image
docker build -t web_mfa_image .
docker stop web_mfa
docker rm web_mfa
docker run -d --restart always --network docker_net  -p 8111:8000 --name web_mfa web_mfa_image
