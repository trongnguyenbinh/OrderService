pipeline {
    agent {
        label 'local'
    }
    
    environment {
        DOCKER_IMAGE = 'your-registry/legacy-order-service'
        IMAGE_TAG = "${BUILD_NUMBER}"
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
                                -p 8080:8080 \
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
                    // Wait for container to be healthy
                    sh """
                        timeout 60 bash -c 'until docker exec legacy-order-service wget --no-verbose --tries=1 --spider http://localhost:8080/api/health 2>&1 | grep -q "200 OK\\|HTTP"; do sleep 2; done'
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