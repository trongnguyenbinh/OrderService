pipeline {
    agent { label 'local' }

    environment {
        DOCKER_IMAGE = 'legacy-order-service'
        IMAGE_TAG    = "${BUILD_NUMBER}"
        EXPOSE_PORT  = 6868
    }

    stages {

        stage('Build Docker Image') {
            steps {
                sh """
                  docker build -t ${DOCKER_IMAGE}:${IMAGE_TAG} .
                  docker tag ${DOCKER_IMAGE}:${IMAGE_TAG} ${DOCKER_IMAGE}:latest
                """
            }
        }

        stage('Run Container') {
            steps {
                withCredentials([
                    string(credentialsId: 'vault-token-connection-string', variable: 'VAULT_TOKEN'),
                    string(credentialsId: 'vault-address', variable: 'VAULT_ADDRESS')
                ]) {
                    sh """
                      docker stop legacy-order-service || true
                      docker rm legacy-order-service || true

                      docker run -d \
                        --name legacy-order-service \
                        -p 127.0.0.1:${EXPOSE_PORT}:8080 \
                        -e VAULT__TOKEN=${VAULT_TOKEN} \
                        -e VAULT__ADDRESS=${VAULT_ADDRESS} \
                        -e TZ=Asia/Bangkok \
                        --restart unless-stopped \
                        ${DOCKER_IMAGE}:${IMAGE_TAG}
                    """
                }
            }
        }

        stage('Health Check') {
            steps {
                sh """
                  timeout 60 bash -c '
                    until [ "\$(docker inspect -f {{.State.Health.Status}} legacy-order-service)" = "healthy" ];
                    do sleep 2; done
                  '
                  echo "Service is healthy"
                """
            }
        }
    }

    post {
        failure {
            echo '🚨 DEPLOY FAILED'
            sh 'docker logs legacy-order-service || true'
        }
        success {
            echo '🚀 DEPLOY SUCCESS'
        }
    }
}
