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
    // CSJ (穿山甲) SDK — China domestic version
    // Update version as needed from: https://www.csjplatform.com/supportcenter/5395
    implementation("com.pangle.cn:ads-sdk-pro:7.4.2.2")
}
