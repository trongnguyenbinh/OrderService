pipeline {
    agent {
        label 'local'
    }

    environment {
        // -----------------------------
        // Docker
        // -----------------------------
        DOCKER_IMAGE = 'legacy-order-service'
        IMAGE_TAG    = "${BUILD_NUMBER}"
        EXPOSE_PORT  = 6868

        // -----------------------------
        // SonarQube
        // -----------------------------
        SONAR_HOST_URL   = 'http://sonar.your-domain:9000'
        SONAR_PROJECT_KEY = 'trongnguyenbinh_OrderService_afb74bb9-e103-4874-944c-ed77f61d9464'
    }

    stages {

        // ==================================================
        // STEP 4 – QUALITY GATE (BLOCK DEPLOY)
        // ==================================================
        stage('Verify SonarQube Quality Gate') {
            steps {
                withCredentials([string(credentialsId: 'sonar-token', variable: 'SONAR_TOKEN')]) {
                    sh '''
                        echo "🔍 Checking SonarQube Quality Gate..."

                        STATUS=$(curl -s -u ${SONAR_TOKEN}: \
                          "${SONAR_HOST_URL}/api/qualitygates/project_status?projectKey=${SONAR_PROJECT_KEY}" \
                          | jq -r '.projectStatus.status')

                        echo "Quality Gate status: $STATUS"

                        if [ "$STATUS" != "OK" ]; then
                          echo "❌ Quality Gate FAILED. Abort deployment."
                          exit 1
                        fi

                        echo "✅ Quality Gate PASSED. Continue pipeline."
                    '''
                }
            }
        }

        // ==================================================
        // BUILD
        // ==================================================
        stage('Build Docker Image') {
            steps {
                sh """
                    docker build -t ${DOCKER_IMAGE}:${IMAGE_TAG} .
                    docker tag ${DOCKER_IMAGE}:${IMAGE_TAG} ${DOCKER_IMAGE}:latest
                """
            }
        }

        // ==================================================
        // DEPLOY
        // ==================================================
        stage('Run Container') {
            steps {
                withCredentials([
                    string(credentialsId: 'vault-token-connection-string', variable: 'VAULT_TOKEN'),
                    string(credentialsId: 'vault-address', variable: 'VAULT_ADDRESS')
                ]) {
                    sh """
                        echo "🚀 Deploying container..."

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

        // ==================================================
        // HEALTH CHECK
        // ==================================================
        stage('Health Check') {
            steps {
                sh """
                    echo "🩺 Waiting for container to become healthy..."
                    timeout 60 bash -c '
                      until [ "\$(docker inspect -f {{.State.Health.Status}} legacy-order-service)" = "healthy" ];
                      do sleep 2; done
                    '
                    echo "✅ Service is healthy!"
                """
            }
        }
    }

    post {
        failure {
            echo '🚨 Pipeline FAILED – Deploy was blocked or unhealthy'
            sh 'docker logs legacy-order-service || true'
        }
        success {
            echo '🎉 Pipeline SUCCESS – Service deployed safely'
        }
    }
}
