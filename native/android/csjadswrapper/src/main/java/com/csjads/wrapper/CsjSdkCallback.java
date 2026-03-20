package com.csjads.wrapper;

/**
 * Callback for SDK initialization result.
 */
public interface CsjSdkCallback {
    void onSuccess();
    void onFailed(int code, String message);
}
