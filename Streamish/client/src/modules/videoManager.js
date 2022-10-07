import firebase from "firebase/app";
import { getToken } from "./authManager";
import "firebase/auth";

const baseUrl = '/api/video';

export const getAllVideos = () => {
  return getToken().then((token) => {
    return fetch(baseUrl, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${token}`,
      },
    })
      .then((res) => res.json())

  })
};

export const addVideo = (video) => {
  return fetch(baseUrl, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      //Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(video),
  });
};