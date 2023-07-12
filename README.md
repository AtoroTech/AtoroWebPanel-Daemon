![Logo](https://i.imgur.com/xI3GLFc.jpeg)
# McControllerX-Daemon
The daemon for McControllerX
 
## THIS PRODUCT IS NOT DONE YET DO NOT USE IT

## Authors

- [@NaysKutzu](https://github.com/NaysKutzu)


## API Reference

Response codes that you can get if you use our api: 
| Code | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `200` | `OK` | Indicates that the request has succeeded. |
| `201` | `Created` | Indicates that the request has succeeded and a new resource has been created as a result. |
| `202` | `Accepted` | Indicates that the request has been received but not completed yet. It is typically used in log running requests and batch processing. |
| `400` | `Bad Request` | The request could not be understood by the server due to incorrect syntax. The client SHOULD NOT repeat the request without modifications. |
| `401` | `Unauthorized` | Indicates that the request requires user authentication information. The client MAY repeat the request with a suitable Authorization header field |
| `403` | `Forbidden` | Unauthorized request. The client does not have access rights to the content. Unlike 401, the client’s identity is known to the server. |
| `404` | `Not Found` | The server can not find the requested resource.|
| `405` | `Method Not Allowed` | The request HTTP method is known by the server but has been disabled and cannot be used for that resource. |
| `500` | `Internal Server Error` | The server encountered an unexpected condition that prevented it from fulfilling the request. |

### System
`GET /api/system`
Returns the system information for the host that our daemon is running on.

### Responses

| Code | Description                                  |
| ---- | -------------------------------------------- |
| 200  | The request was successful.                  |
| 400  | The system information could not be fetched. |


## Suport us
You can support us via PayPal at:

https://paypal.me/mythicalsystems

## Contributing

You are allowed to contribute, and we are very thankful if you help us with the project, and you can add your own copyright in the LICENSE file however we do not allow you to make a project under other name and remove or license and still use our lines of code 

!! WE WILL DMCA YOU !!


## License

[MIT](https://choosealicense.com/licenses/mit/)
