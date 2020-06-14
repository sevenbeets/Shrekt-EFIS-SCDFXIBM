#include <WiFi.h>
#include <PubSubClient.h>

#include <Servo.h>

/* LEDS */
#define LED_GREEN   21
#define LED_RED     23

/* TEMPERATURE SENSOR */
#define TEMP        35

/* SERVO MOTOR */
#define SERVO       13

/* SSID */
#define SSID        "CID Surveillance Van"
#define PASSWORD    "canthackthis"

/* MQTT BROKER */
#define BROKER      "192.168.43.42"

WiFiClient espClient;
PubSubClient client(espClient);

Servo servo;
int old_pos = 0;
int new_pos = 0;

double temp;

void setup() {
  pinMode(LED_GREEN, OUTPUT);
  pinMode(LED_RED, OUTPUT);
  pinMode(TEMP, INPUT);
  
  servo.attach(SERVO);
  servo.write(0);
  
  setup_wifi();
  client.setServer(BROKER, 1883);
  client.setCallback(callback);
  
  Serial.begin(9600);
}

void setup_wifi() {
  delay(10);
  // We start by connecting to a WiFi network
  Serial.println();
  Serial.print("Connecting to ");
  Serial.println(SSID);

  WiFi.begin(SSID, PASSWORD);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
}

void callback(char* topic, byte* message, unsigned int length) {
  Serial.print("Message arrived on topic: ");
  Serial.print(topic);
  Serial.print(". Message: ");
  String messageTemp;
  
  for (int i = 0; i < length; i++) {
    Serial.print((char)message[i]);
    messageTemp += (char)message[i];
  }
  Serial.println();
  
  if (String(topic) == "scdf/out") {
    Serial.print("Changing output to ");
    if(messageTemp == "on"){
      Serial.println("on");
      digitalWrite(LED_GREEN, HIGH);
    }
    else if(messageTemp == "off"){
      Serial.println("off");
      digitalWrite(LED_GREEN, LOW);
    }
  }
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected()) {
    Serial.print("Attempting MQTT connection...");
    // Attempt to connect
    if (client.connect("ESP8266Client")) {
      Serial.println("connected");
      // Subscribe
      client.subscribe("scdf/out");
    } else {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

int heartbeat = 0;
int fire = 0;

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

  if (heartbeat++ >= 2) {
    client.publish("device/status", "1");
    digitalWrite(LED_GREEN, HIGH);
    heartbeat = 0;
  } else {
    digitalWrite(LED_GREEN, LOW);
  }

  char tempString[8];
  double temp = ((analogRead(TEMP) / 1023.0) - 0.5) * 100;
  dtostrf(temp, 1, 2, tempString);
  client.publish("device/temp", tempString);
  Serial.println(temp);
  
  // TODO: Proper thresholding and hysteresis
  if (temp >= 29) {
    if (fire < 5) fire++;
  } else {
    // OFF
    if (fire > 0) fire--;
  }

  if (fire >= 5) {
    // ON
    digitalWrite(LED_RED, HIGH);
    client.publish("device/fire", "1");
    new_pos = 180;
  } else if (fire <= 0) {
    // OFF
    digitalWrite(LED_RED, LOW);
    client.publish("device/fire", "0");
    new_pos = 0;
  }

  if (new_pos != old_pos) {
    servo.write(new_pos);
    old_pos = new_pos;
  }

  delay(500);
}
