pipeline {
    agent {
        label 'local'
    }
    
    environment {
        DOCKER_IMAGE = 'legacy-order-service'
        IMAGE_TAG = "${BUILD_NUMBER}"
        EXPOSE_PORT = 6868
    }
    
    stages {
        stage('Build Docker Image') {
            steps {
                script {
                    sh """
                        docker build -t ${DOCKER_IMAGE}:${IMAGE_TAG} .
                        docker tag ${DOCKER_IMAGE}:${IMAGE_TAG} ${DOCKER_IMAGE}:latest
                    """
                }
            }
        }
        
        stage('Run Container') {
            steps {
                script {
                    // Get Vault token from Jenkins credentials
                    withCredentials([string(credentialsId: 'vault-token-connection-string', variable: 'VAULT_TOKEN')]) {
                        sh """
                            # Stop and remove existing container if it exists
                            docker stop legacy-order-service || true
                            docker rm legacy-order-service || true
                            
                            # Run new container with Vault token
                            docker run -d \
                                --name legacy-order-service \
                                -p ${EXPOSE_PORT}:8080 \
                                -e VAULT__TOKEN=${VAULT_TOKEN} \
                                -e ASPNETCORE_ENVIRONMENT=Production \
                                -e TZ=Asia/Bangkok \
                                --restart unless-stopped \
                                ${DOCKER_IMAGE}:${IMAGE_TAG}
                        """
                    }
                }
            }
        }
        
        stage('Health Check') {
            steps {
                script {
                    sh """
                        timeout 60 bash -c 'until [ "\$(docker inspect -f {{.State.Health.Status}} legacy-order-service)" = "healthy" ]; do sleep 2; done'
                        echo "Service is healthy!"
                    """
                }
            }
        }
    }
    
    post {
        failure {
            script {
                // Show container logs on failure
                sh 'docker logs legacy-order-service || true'
            }
        }
    }
}