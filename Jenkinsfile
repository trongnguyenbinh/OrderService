pipeline {
    agent { label 'local' }

    parameters {
        string(name: 'analysisId', description: 'SonarQube analysisId')
    }

    environment {
        DOCKER_IMAGE = 'legacy-order-service'
        IMAGE_TAG    = "${BUILD_NUMBER}"
        EXPOSE_PORT  = 6868
    }

    stages {

        stage('Verify SonarQube Quality Gate (analysisId)') {
            steps {
                withCredentials([
                    string(credentialsId: 'sonar-token', variable: 'SONAR_TOKEN'),
                    string(credentialsId: 'sonar-host-url', variable: 'SONAR_HOST_URL')
                ]) {
                    sh '''
                      echo "🔍 Checking SonarQube Quality Gate"
                      echo "analysisId = ${analysisId}"

                      STATUS=$(curl -s -u ${SONAR_TOKEN}: \
                        "${SONAR_HOST_URL}/api/qualitygates/project_status?analysisId=${analysisId}" \
                        | jq -r '.projectStatus.status')

                      echo "Quality Gate status: $STATUS"

                      if [ "$STATUS" != "OK" ]; then
                        echo "❌ Quality Gate FAILED"
                        exit 1
                      fi

                      echo "✅ Quality Gate PASSED"
                    '''
                }
            }
        }

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
            echo '🚨 DEPLOY BLOCKED'
            sh 'docker logs legacy-order-service || true'
        }
        success {
            echo '🚀 DEPLOY SUCCESS'
        }
    }
}
