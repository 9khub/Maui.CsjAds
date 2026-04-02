plugins {
    id("com.android.library")
}

android {
    namespace = "com.csjads.wrapper"
    compileSdk = 36

    defaultConfig {
        minSdk = 24
        consumerProguardFiles("proguard-rules.pro")
    }

    buildTypes {
        release {
            isMinifyEnabled = false
        }
    }

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
}

dependencies {
    // 穿山甲融合 SDK（GroMore）：内含穿山甲 + 聚合能力；与 ads-sdk-pro 勿同时引入，避免 lib 重复。
    // Maven 版本以 https://artifact.bytedance.com/repository/pangle 为准（当前 mediation-sdk 最新 7.4.2.1）。
    implementation("com.pangle.cn:mediation-sdk:7.4.2.1")
    implementation("com.squareup.okhttp3:okhttp:3.12.1")
}
