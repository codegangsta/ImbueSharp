using System;

namespace ZaaLabs.Imbue
{
    /**
     * IInjector is an interface that provides a minimalist dependency 
     * injection API.
     */
    interface IInjector
    {
        /**
         * Maps a value to a certain class type, so when Apply()
         * is called, the value will be injected into any property with the
         * [Inject] tag over it and that matches the class definition specified
         * in the type parameter.
         */
        void MapValue(Object value, Type type);

        /**
         * Applies the injection to the specified object. If you wish to have a
         * property injected into the specified object, make sure that an [Inject]
         * metadata tag is added to the desired target property and the value is
         * mapped in this injector using the MapValue() method.
         */
        void Apply(Object value);
    }
}
