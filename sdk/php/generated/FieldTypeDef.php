<?php

/**
 * This class has been generated by dagger-php-sdk. DO NOT EDIT.
 */

declare(strict_types=1);

namespace Dagger;

/**
 * A definition of a field on a custom object defined in a Module.
 * A field on an object has a static value, as opposed to a function on an
 * object whose value is computed by invoking code (and can accept arguments).
 */
class FieldTypeDef extends Client\AbstractObject
{
    /**
     * A doc string for the field, if any
     */
    public function description(): string
    {
        $leafQueryBuilder = new \Dagger\Client\QueryBuilder('description');
        return (string)$this->queryLeaf($leafQueryBuilder, 'description');
    }

    /**
     * The name of the field in the object
     */
    public function name(): string
    {
        $leafQueryBuilder = new \Dagger\Client\QueryBuilder('name');
        return (string)$this->queryLeaf($leafQueryBuilder, 'name');
    }

    /**
     * The type of the field
     */
    public function typeDef(): TypeDef
    {
        $innerQueryBuilder = new \Dagger\Client\QueryBuilder('typeDef');
        return new \Dagger\TypeDef($this->client, $this->queryBuilderChain->chain($innerQueryBuilder));
    }
}
