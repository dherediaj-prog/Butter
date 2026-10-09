import {MessageSender} from './Entities/Network/MessageSenders/MessageSender';
import {MessageDispatcher} from './Entities/Network/MessageHandler/MessageDispatcher.js';
import {PromptMessageHandler} from './Entities/Network/MessageHandler/Implementations/PromptMessageHandler';
import {CancelMessageHandler} from './Entities/Network/MessageHandler/Implementations/CancelMessageHandler';
import {GeminiProvider} from './Entities/IAProviders/GeminiProvider';

// 1. Instanciar el emisor (salida) y el proveedor de IA del DOM
const sender = new MessageSender();
const provider = new GeminiProvider();

// 2. Instanciar el despachador (escucha entradas del worker y usa 'sender' para responder)
const dispatcher = new MessageDispatcher(sender, [
    new PromptMessageHandler(provider),
    new CancelMessageHandler(provider)
]);